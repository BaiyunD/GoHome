using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ShopService : MonoBehaviour
{
    public static ShopService Instance
    {
        get; private set;
    }

    public event Action ShopStateChanged;

    [SerializeField] private bool shopFeatureEnabled = true;

    private readonly ShopWallet _wallet = new ShopWallet();
    private readonly InventoryItemFacade _inventoryItemFacade = new InventoryItemFacade();
    private IShopPointsPolicy _pointsPolicy = new DecimalPricePointsPolicy();
    private IShopTransactionPolicy _transactionPolicy = new DefaultShopTransactionPolicy();
    private readonly Dictionary<int, ShopCommodityDefinition> _commoditiesById = new Dictionary<int, ShopCommodityDefinition>();
    private readonly Dictionary<int, float> _runtimeBuyPrices = new Dictionary<int, float>();

    public bool IsFeatureEnabled => shopFeatureEnabled;

    public int Points => _wallet.Points;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void InitializeForNewGame()
    {
        _wallet.InitializeForNewGame();
        _runtimeBuyPrices.Clear();
        NotifyStateChanged();
    }

    public void ApplySnapshot(ShopWalletSnapshot snapshot)
    {
        _wallet.ApplySnapshot(snapshot);
        _runtimeBuyPrices.Clear();
        ShopBuyPriceSnapshot[] priceSnapshots = snapshot.BuyPriceSnapshots;
        if (priceSnapshots != null)
        {
            for (int i = 0; i < priceSnapshots.Length; i++)
            {
                ShopBuyPriceSnapshot priceSnapshot = priceSnapshots[i];
                if (priceSnapshot.CommodityId <= 0)
                {
                    continue;
                }

                _runtimeBuyPrices[priceSnapshot.CommodityId] = NormalizeMoney(priceSnapshot.CurrentBuyPrice);
            }
        }

        NotifyStateChanged();
    }

    public ShopWalletSnapshot ExportSnapshot()
    {
        ShopWalletSnapshot snapshot = _wallet.ExportSnapshot();
        ShopBuyPriceSnapshot[] priceSnapshots = new ShopBuyPriceSnapshot[_runtimeBuyPrices.Count];
        int index = 0;
        foreach (KeyValuePair<int, float> pair in _runtimeBuyPrices)
        {
            priceSnapshots[index++] = new ShopBuyPriceSnapshot
            {
                CommodityId = pair.Key,
                CurrentBuyPrice = NormalizeMoney(pair.Value)
            };
        }

        snapshot.BuyPriceSnapshots = priceSnapshots;
        return snapshot;
    }

    public bool TryGetCash(out float cash)
    {
        cash = 0f;
        if (PlayerResourceService.Instance == null)
        {
            return false;
        }

        return PlayerResourceService.Instance.TryGetValue(PlayerResourceType.Money, out cash);
    }

    public IReadOnlyList<ShopCommodityViewModel> BuildCommodityViewModels(IReadOnlyList<ShopCommodityDefinition> commodities)
    {
        List<ShopCommodityViewModel> viewModels = new List<ShopCommodityViewModel>();
        if (commodities == null)
        {
            return viewModels;
        }

        _commoditiesById.Clear();

        for (int i = 0; i < commodities.Count; i++)
        {
            ShopCommodityDefinition commodity = commodities[i];
            if (commodity == null)
            {
                continue;
            }

            if (commodity.CommodityId > 0)
            {
                _commoditiesById[commodity.CommodityId] = commodity;
            }

            _inventoryItemFacade.TryGetSnapshot(commodity.ItemId, out InventoryItemSnapshot itemSnapshot);
            bool canBuy = IsFeatureEnabled &&
                (commodity.TradePermission == ShopTradePermission.BuyAndSell ||
                commodity.TradePermission == ShopTradePermission.BuyOnly);
            bool canSell = IsFeatureEnabled &&
                (commodity.TradePermission == ShopTradePermission.BuyAndSell ||
                commodity.TradePermission == ShopTradePermission.SellOnly);
            ShopCommodityViewModel viewModel = new ShopCommodityViewModel
            {
                CommodityId = commodity.CommodityId,
                ItemId = commodity.ItemId,
                ItemName = itemSnapshot.ItemName,
                ItemDescription = itemSnapshot.ItemDescription,
                Icon = itemSnapshot.ItemIcon,
                OwnedCount = itemSnapshot.OwnedCount,
                TradeCount = commodity.TradeCount,
                BuyPrice = ResolveCurrentBuyPrice(commodity),
                SellPrice = commodity.SellPrice,
                CanBuy = canBuy,
                CanSell = canSell,
                IsSellAll = commodity.IsSellAll
            };
            viewModels.Add(viewModel);
        }

        return viewModels;
    }

    public ShopTradeResult ExecuteTrade(ShopTradeRequest request)
    {
        ShopTradeOperation operation = request.Operation;
        if (!IsFeatureEnabled)
        {
            return BuildFailure(ShopTradeReasonCode.FeatureDisabled, operation);
        }

        if (!_transactionPolicy.Validate(request, out ShopTradeReasonCode reasonCode))
        {
            return BuildFailure(reasonCode, operation);
        }

        if (!_commoditiesById.TryGetValue(request.CommodityId, out ShopCommodityDefinition commodity) || commodity == null)
        {
            return BuildFailure(ShopTradeReasonCode.CommodityMissing, operation);
        }

        bool canBuy = commodity.TradePermission == ShopTradePermission.BuyAndSell
            || commodity.TradePermission == ShopTradePermission.BuyOnly;
        bool canSell = commodity.TradePermission == ShopTradePermission.BuyAndSell
            || commodity.TradePermission == ShopTradePermission.SellOnly;
        int itemId = commodity.ItemId;
        int tradeCount = commodity.TradeCount;
        float buyPricePerUnit = ResolveCurrentBuyPrice(commodity);
        float sellPricePerUnit = commodity.SellPrice;
        string itemName = ResolveItemName(itemId);

        switch (request.Operation)
        {
            case ShopTradeOperation.Buy:
                if (!canBuy)
                {
                    return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
                }

                if (tradeCount <= 0 || request.Times <= 0)
                {
                    return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
                }

                int buyCount = tradeCount * request.Times;
                float buyTotalPrice = NormalizeMoney(buyPricePerUnit * buyCount);
                if (PlayerResourceService.Instance == null)
                {
                    return BuildFailure(ShopTradeReasonCode.InsufficientCash, request.Operation);
                }

                if (!PlayerResourceService.Instance.TrySpendMoney(buyTotalPrice, "ShopService.ExecuteTrade.Buy"))
                {
                    return BuildFailure(ShopTradeReasonCode.InsufficientCash, request.Operation);
                }

                if (InventoryManager.Instance == null)
                {
                    return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
                }

                InventoryManager.Instance.AddItem(itemId, buyCount);
                int buyPoints = CalculatePoints(buyTotalPrice);
                _wallet.AddPoints(buyPoints);
                ApplyBuyOnlyPriceIncreaseIfNeeded(commodity, buyPricePerUnit);
                NotifyStateChanged();
                return new ShopTradeResult
                {
                    Success = true,
                    ReasonCode = ShopTradeReasonCode.Success,
                    DeltaCash = -buyTotalPrice,
                    DeltaPoints = buyPoints,
                    ItemName = itemName,
                    TradeCount = buyCount,
                    TotalPrice = buyTotalPrice,
                    Operation = request.Operation,
                    DeltaItems = new ShopItemDelta
                    {
                        ItemId = itemId,
                        DeltaCount = buyCount
                    }
                };

            case ShopTradeOperation.Sell:
                if (!canSell)
                {
                    return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
                }

                if (InventoryManager.Instance == null)
                {
                    return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
                }

                bool isSellOnly = commodity.TradePermission == ShopTradePermission.SellOnly;
                int sellCount;
                if (isSellOnly && commodity.IsSellAll)
                {
                    // SellOnly + IsSellAll：卖出“全部剩余存货”（以库存为准）。
                    sellCount = InventoryManager.Instance.GetItemCount(itemId);
                }
                else
                {
                    if (tradeCount <= 0 || request.Times <= 0)
                    {
                        return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
                    }

                    sellCount = tradeCount * request.Times;
                }

                if (sellCount <= 0)
                {
                    return BuildFailure(ShopTradeReasonCode.InsufficientInventory, request.Operation);
                }

                int currentOwned = InventoryManager.Instance.GetItemCount(itemId);
                if (currentOwned < sellCount)
                {
                    return BuildFailure(ShopTradeReasonCode.InsufficientInventory, request.Operation);
                }

                float sellTotalPrice = NormalizeMoney(sellPricePerUnit * sellCount);
                if (PlayerResourceService.Instance == null)
                {
                    return BuildFailure(ShopTradeReasonCode.InsufficientCash, request.Operation);
                }

                if (!PlayerResourceService.Instance.ApplyDelta(
                        PlayerResourceType.Money,
                        sellTotalPrice,
                        "ShopService.ExecuteTrade.Sell"))
                {
                    return BuildFailure(ShopTradeReasonCode.InsufficientCash, request.Operation);
                }

                InventoryManager.Instance.RemoveItem(itemId, sellCount);
                int sellPoints = CalculatePoints(sellTotalPrice);
                _wallet.AddPoints(sellPoints);
                NotifyStateChanged();
                return new ShopTradeResult
                {
                    Success = true,
                    ReasonCode = ShopTradeReasonCode.Success,
                    DeltaCash = sellTotalPrice,
                    DeltaPoints = sellPoints,
                    ItemName = itemName,
                    TradeCount = sellCount,
                    TotalPrice = sellTotalPrice,
                    Operation = request.Operation,
                    DeltaItems = new ShopItemDelta
                    {
                        ItemId = itemId,
                        DeltaCount = -sellCount
                    }
                };

            default:
                return BuildFailure(ShopTradeReasonCode.InvalidRequest, request.Operation);
        }
    }

    public int CalculatePoints(float pricePerTrade)
    {
        if (_pointsPolicy == null)
        {
            return 0;
        }

        return _pointsPolicy.CalculatePoints(pricePerTrade);
    }

    private const int LotteryCostPoints = 100;
    private const float LotteryWeightEpsilon = 0.001f;

    public ShopLotteryDrawResult TryDrawLottery(ShopLotteryPoolDefinition pool)
    {
        if (!IsFeatureEnabled)
        {
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.InvalidPool,
                DetailMessage = "商店功能未开启。"
            };
        }

        if (pool == null || pool.Tiers == null || pool.Tiers.Count == 0)
        {
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.InvalidPool,
                DetailMessage = "奖池未配置或为空。"
            };
        }

        if (_wallet.Points < LotteryCostPoints)
        {
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.InsufficientPoints
            };
        }

        if (!TryValidateLotteryPool(pool, out string invalidMessage))
        {
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.InvalidPool,
                DetailMessage = invalidMessage
            };
        }

        float sumWeights = pool.SumTierWeights();
        float thanksWeight = Mathf.Max(0f, 100f - sumWeights);

        if (!_wallet.TrySpendPoints(LotteryCostPoints))
        {
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.InsufficientPoints
            };
        }

        int bucket = UnityEngine.Random.Range(0, 10000);
        int thanksBuckets = Mathf.Clamp(Mathf.RoundToInt(thanksWeight * 100f), 0, 10000);
        if (bucket < thanksBuckets)
        {
            NotifyStateChanged();
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.RolledThanks
            };
        }

        int tierSpan = 10000 - thanksBuckets;
        int tierRoll = bucket - thanksBuckets;
        ShopLotteryTierData chosenTier = null;
        if (tierSpan <= 0 || sumWeights <= LotteryWeightEpsilon)
        {
            _wallet.RefundPoints(LotteryCostPoints);
            NotifyStateChanged();
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.GrantFailed,
                DetailMessage = "抽奖过程异常，积分已退回。"
            };
        }

        float normalized = tierRoll / (float)tierSpan;
        float cumulative = 0f;
        ShopLotteryTierData lastWeightedTier = null;
        for (int i = 0; i < pool.Tiers.Count; i++)
        {
            ShopLotteryTierData tier = pool.Tiers[i];
            if (tier == null)
            {
                continue;
            }

            float w = tier.WeightPercent;
            if (w <= 0f)
            {
                continue;
            }

            lastWeightedTier = tier;
            cumulative += w / sumWeights;
            if (normalized < cumulative)
            {
                chosenTier = tier;
                break;
            }
        }

        if (chosenTier == null)
        {
            chosenTier = lastWeightedTier;
        }

        if (chosenTier == null)
        {
            _wallet.RefundPoints(LotteryCostPoints);
            NotifyStateChanged();
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.GrantFailed,
                DetailMessage = "抽奖过程异常，积分已退回。"
            };
        }

        if (!TryPickRandomRewardOption(chosenTier, out ShopLotteryRewardOptionData option))
        {
            _wallet.RefundPoints(LotteryCostPoints);
            NotifyStateChanged();
            return new ShopLotteryDrawResult
            {
                Reason = ShopLotteryDrawReason.GrantFailed,
                DetailMessage = "奖励抽取异常，积分已退回。"
            };
        }

        if (!TryGrantLotteryReward(option, out ShopLotteryDrawResult grantResult))
        {
            _wallet.RefundPoints(LotteryCostPoints);
            NotifyStateChanged();
            return grantResult;
        }

        grantResult.TierDisplayName = chosenTier.DisplayName;
        NotifyStateChanged();
        return grantResult;
    }

    private static bool TryValidateLotteryPool(ShopLotteryPoolDefinition pool, out string message)
    {
        message = string.Empty;
        float p = pool.SumTierWeights();
        if (p > 100f + LotteryWeightEpsilon)
        {
            message = "奖池档位概率之和超过100%，请调整奖池后再抽奖。";
            return false;
        }

        float thanksRemainder = Mathf.Max(0f, 100f - p);
        if (thanksRemainder > LotteryWeightEpsilon)
        {
            for (int i = 0; i < pool.Tiers.Count; i++)
            {
                ShopLotteryTierData tier = pool.Tiers[i];
                if (tier == null)
                {
                    continue;
                }

                if (IsReservedThanksName(tier.DisplayName))
                {
                    message = "存在隐式「谢谢惠顾」余量时，档位名称不能使用「谢谢惠顾」。请将档位概率之和设为100%或修改档位名称。";
                    return false;
                }
            }
        }

        for (int i = 0; i < pool.Tiers.Count; i++)
        {
            ShopLotteryTierData tier = pool.Tiers[i];
            if (tier == null)
            {
                continue;
            }

            if (tier.WeightPercent <= 0f)
            {
                continue;
            }

            if (!TierHasAnyValidReward(tier))
            {
                message = "奖池中某档位概率大于0但未配置有效奖励，请检查后重试。";
                return false;
            }
        }

        return true;
    }

    private static bool IsReservedThanksName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        return string.Equals(displayName.Trim(), ShopLotteryPoolDefinition.ReservedThanksDisplayName, StringComparison.Ordinal);
    }

    private static bool TierHasAnyValidReward(ShopLotteryTierData tier)
    {
        IReadOnlyList<ShopLotteryRewardOptionData> options = tier.RewardOptions;
        if (options == null || options.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < options.Count; i++)
        {
            ShopLotteryRewardOptionData opt = options[i];
            if (opt == null)
            {
                continue;
            }

            if (IsValidRewardOption(opt))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidRewardOption(ShopLotteryRewardOptionData option)
    {
        switch (option.Kind)
        {
            case ShopLotteryRewardKind.Item:
                return option.ItemId > 0 && option.ItemCount > 0;
            case ShopLotteryRewardKind.Money:
                return MoneyUtil.YuanToCents(option.MoneyAmount) > 0;
            default:
                return false;
        }
    }

    private static bool TryPickRandomRewardOption(ShopLotteryTierData tier, out ShopLotteryRewardOptionData option)
    {
        option = null;
        IReadOnlyList<ShopLotteryRewardOptionData> options = tier.RewardOptions;
        if (options == null || options.Count == 0)
        {
            return false;
        }

        List<int> validIndices = new List<int>();
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null && IsValidRewardOption(options[i]))
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0)
        {
            return false;
        }

        int pick = validIndices[UnityEngine.Random.Range(0, validIndices.Count)];
        option = options[pick];
        return option != null;
    }

    private bool TryGrantLotteryReward(ShopLotteryRewardOptionData option, out ShopLotteryDrawResult result)
    {
        result = default;
        switch (option.Kind)
        {
            case ShopLotteryRewardKind.Item:
                if (InventoryManager.Instance == null)
                {
                    result = new ShopLotteryDrawResult
                    {
                        Reason = ShopLotteryDrawReason.GrantFailed,
                        DetailMessage = "背包系统未就绪，积分已退回。"
                    };
                    return false;
                }

                InventoryManager.Instance.AddItem(option.ItemId, option.ItemCount);
                result = new ShopLotteryDrawResult
                {
                    Reason = ShopLotteryDrawReason.RolledTier,
                    RewardKind = ShopLotteryRewardKind.Item,
                    ItemId = option.ItemId,
                    ItemCount = option.ItemCount,
                    ItemDisplayName = ResolveItemName(option.ItemId)
                };
                return true;

            case ShopLotteryRewardKind.Money:
                float money = NormalizeMoney(option.MoneyAmount);
                if (money <= 0f)
                {
                    result = new ShopLotteryDrawResult
                    {
                        Reason = ShopLotteryDrawReason.GrantFailed,
                        DetailMessage = "金钱奖励无效，积分已退回。"
                    };
                    return false;
                }

                if (PlayerResourceService.Instance == null)
                {
                    result = new ShopLotteryDrawResult
                    {
                        Reason = ShopLotteryDrawReason.GrantFailed,
                        DetailMessage = "玩家资源服务未就绪，积分已退回。"
                    };
                    return false;
                }

                if (!PlayerResourceService.Instance.ApplyDelta(
                        PlayerResourceType.Money,
                        money,
                        "ShopService.TryDrawLottery.MoneyReward"))
                {
                    result = new ShopLotteryDrawResult
                    {
                        Reason = ShopLotteryDrawReason.GrantFailed,
                        DetailMessage = "金钱发放失败，积分已退回。"
                    };
                    return false;
                }

                result = new ShopLotteryDrawResult
                {
                    Reason = ShopLotteryDrawReason.RolledTier,
                    RewardKind = ShopLotteryRewardKind.Money,
                    MoneyAmount = money
                };
                return true;

            default:
                result = new ShopLotteryDrawResult
                {
                    Reason = ShopLotteryDrawReason.GrantFailed,
                    DetailMessage = "未知奖励类型，积分已退回。"
                };
                return false;
        }
    }

    private static ShopTradeResult BuildFailure(ShopTradeReasonCode reasonCode, ShopTradeOperation operation)
    {
        return new ShopTradeResult
        {
            Success = false,
            ReasonCode = reasonCode,
            DeltaCash = 0f,
            DeltaPoints = 0,
            DeltaItems = default,
            ItemName = string.Empty,
            TradeCount = 0,
            TotalPrice = 0f,
            Operation = operation
        };
    }

    private static string ResolveItemName(int itemId)
    {
        if (ItemRegistry.Instance != null &&
            ItemRegistry.Instance.TryGet(itemId, out ItemBase item) &&
            item != null)
        {
            return item.DisplayName;
        }

        return $"Item({itemId})";
    }

    private void NotifyStateChanged()
    {
        ShopStateChanged?.Invoke();
    }

    private float ResolveCurrentBuyPrice(ShopCommodityDefinition commodity)
    {
        if (commodity == null)
        {
            return 0f;
        }

        float basePrice = NormalizeMoney(commodity.BuyPrice);
        if (!IsDynamicBuyPriceCommodity(commodity))
        {
            return basePrice;
        }

        if (_runtimeBuyPrices.TryGetValue(commodity.CommodityId, out float runtimePrice))
        {
            return NormalizeMoney(runtimePrice);
        }

        return basePrice;
    }

    private void ApplyBuyOnlyPriceIncreaseIfNeeded(ShopCommodityDefinition commodity, float currentUnitPrice)
    {
        if (!IsDynamicBuyPriceCommodity(commodity))
        {
            return;
        }

        float nextPrice = NormalizeMoney(currentUnitPrice + 0.2f);
        _runtimeBuyPrices[commodity.CommodityId] = nextPrice;
    }

    private static bool IsDynamicBuyPriceCommodity(ShopCommodityDefinition commodity)
    {
        return commodity != null &&
            commodity.TradePermission == ShopTradePermission.BuyOnly &&
            commodity.IsPriceIncreaseOnBuy &&
            commodity.CommodityId > 0;
    }

    private static float NormalizeMoney(float value)
    {
        return MoneyUtil.CentsToYuan(MoneyUtil.YuanToCents(value));
    }
}
