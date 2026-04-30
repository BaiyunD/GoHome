using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class ShopPagePresenter : MonoBehaviour
{
    [Header("Page")]
    [SerializeField] private ShopPage shopPage;

    [Header("Data")]
    [SerializeField] private ShopCatalogDefinition catalogDefinition;
    [SerializeField] private ShopLotteryPoolDefinition lotteryPool;

    [Header("Header")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text pointsText;

    [Header("Grid")]
    [SerializeField] private Transform slotRoot;
    [SerializeField] private ShopCommoditySlot slotPrefab;

    private readonly List<ShopCommoditySlot> _runtimeSlots = new List<ShopCommoditySlot>();
    private readonly List<ShopCommodityViewModel> _runtimeViewModels = new List<ShopCommodityViewModel>();
    private int _selectedCommodityId = -1;

    private void Awake()
    {
        if (shopPage != null)
        {
            shopPage.BindPresenter(this);
        }
    }

    private void OnEnable()
    {
        if (ShopService.Instance != null)
        {
            ShopService.Instance.ShopStateChanged -= RefreshAll;
            ShopService.Instance.ShopStateChanged += RefreshAll;
        }
    }

    private void OnDisable()
    {
        if (ShopService.Instance != null)
        {
            ShopService.Instance.ShopStateChanged -= RefreshAll;
        }
    }

    public void RefreshAll()
    {
        RefreshHeader();
        RefreshGrid();
        EnsureDefaultSelection();
    }

    public void RequestBuySelected()
    {
        if (_selectedCommodityId <= 0)
        {
            NotifyTradeResult(new ShopTradeResult
            {
                Success = false,
                ReasonCode = ShopTradeReasonCode.InvalidRequest,
                Operation = ShopTradeOperation.Buy
            });
            return;
        }

        ExecuteTrade(_selectedCommodityId, ShopTradeOperation.Buy);
    }

    public void RequestSellSelected()
    {
        if (_selectedCommodityId <= 0)
        {
            NotifyTradeResult(new ShopTradeResult
            {
                Success = false,
                ReasonCode = ShopTradeReasonCode.InvalidRequest,
                Operation = ShopTradeOperation.Sell
            });
            return;
        }

        ExecuteTrade(_selectedCommodityId, ShopTradeOperation.Sell);
    }

    public void RequestLottery()
    {
        if (ShopService.Instance == null)
        {
            return;
        }

        ShopLotteryDrawResult result = ShopService.Instance.TryDrawLottery(lotteryPool);
        NotifyLotteryResult(result);
    }

    public void SelectCommodity(int commodityId)
    {
        _selectedCommodityId = commodityId;
        RefreshSelectionState();
        RefreshDetailPanel();
    }

    private void ExecuteTrade(int commodityId, ShopTradeOperation operation)
    {
        if (ShopService.Instance == null)
        {
            return;
        }

        ShopTradeRequest request = new ShopTradeRequest
        {
            CommodityId = commodityId,
            Operation = operation,
            Times = 1
        };

        ShopTradeResult result = ShopService.Instance.ExecuteTrade(request);
        NotifyTradeResult(result);
    }

    private void RefreshHeader()
    {
        float cash = 0f;
        int points = 0;
        if (ShopService.Instance != null)
        {
            ShopService.Instance.TryGetCash(out cash);
            points = ShopService.Instance.Points;
        }

        if (moneyText != null)
        {
            moneyText.text = $"金钱：{cash:0.00}元";
        }
        if (pointsText != null)
        {
            pointsText.text = $"积分：{points}";
        }
    }

    private void RefreshGrid()
    {
        ClearGrid();
        if (slotRoot == null || slotPrefab == null || catalogDefinition == null || ShopService.Instance == null)
        {
            if (shopPage != null)
            {
                shopPage.SetDetailEmptyState();
            }
            return;
        }

        IReadOnlyList<ShopCommodityViewModel> viewModels = ShopService.Instance.BuildCommodityViewModels(catalogDefinition.Commodities);
        for (int i = 0; i < viewModels.Count; i++)
        {
            ShopCommoditySlot slot = Instantiate(slotPrefab, slotRoot);
            slot.Bind(viewModels[i], this);
            _runtimeSlots.Add(slot);
            _runtimeViewModels.Add(viewModels[i]);
        }
    }

    private void ClearGrid()
    {
        for (int i = 0; i < _runtimeSlots.Count; i++)
        {
            if (_runtimeSlots[i] != null)
            {
                Destroy(_runtimeSlots[i].gameObject);
            }
        }

        _runtimeSlots.Clear();
        _runtimeViewModels.Clear();
    }

    private void EnsureDefaultSelection()
    {
        if (_runtimeViewModels.Count <= 0)
        {
            _selectedCommodityId = -1;
            if (shopPage != null)
            {
                shopPage.SetDetailEmptyState();
            }
            return;
        }

        bool hasSelected = false;
        for (int i = 0; i < _runtimeViewModels.Count; i++)
        {
            if (_runtimeViewModels[i].CommodityId == _selectedCommodityId)
            {
                hasSelected = true;
                break;
            }
        }

        if (!hasSelected)
        {
            _selectedCommodityId = _runtimeViewModels[0].CommodityId;
        }

        RefreshSelectionState();
        RefreshDetailPanel();
    }

    private void RefreshSelectionState()
    {
        for (int i = 0; i < _runtimeSlots.Count && i < _runtimeViewModels.Count; i++)
        {
            bool selected = _runtimeViewModels[i].CommodityId == _selectedCommodityId;
            _runtimeSlots[i].SetSelected(selected);
        }
    }

    private void RefreshDetailPanel()
    {
        if (shopPage == null)
        {
            return;
        }

        for (int i = 0; i < _runtimeViewModels.Count; i++)
        {
            if (_runtimeViewModels[i].CommodityId == _selectedCommodityId)
            {
                shopPage.BindDetail(_runtimeViewModels[i]);
                return;
            }
        }

        shopPage.SetDetailEmptyState();
    }

    private static void NotifyTradeResult(ShopTradeResult result)
    {
        if (ShopToastService.Instance != null)
        {
            ShopToastService.Instance.NotifyTradeResult(result);
            return;
        }

        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.ShowResultToast(ShopToastService.BuildMessage(result));
    }

    private static void NotifyLotteryResult(ShopLotteryDrawResult result)
    {
        if (ShopToastService.Instance != null)
        {
            ShopToastService.Instance.NotifyLotteryResult(result);
            return;
        }

        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.ShowResultToast(ShopToastService.BuildLotteryMessage(result));
    }
}
