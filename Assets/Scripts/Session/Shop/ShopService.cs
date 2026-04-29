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
        return Mathf.Round(value * 10f) / 10f;
    }
}
