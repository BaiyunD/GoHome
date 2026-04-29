using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class ShopPage : MonoBehaviour
{
    [SerializeField] private ShopPagePresenter presenter;
    [Header("Detail")]
    [SerializeField] private Image detailIconImage;
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailBuyPriceText;
    [SerializeField] private TMP_Text detailSellPriceText;
    [SerializeField] private Button detailBuyButton;
    [SerializeField] private Button detailSellButton;

    public void Show()
    {
        gameObject.SetActive(true);
        if (presenter != null)
        {
            presenter.RefreshAll();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnClose()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseUIEntry(UIKey.ShopPage);
        }
    }

    public void OnBuy()
    {
        if (presenter != null)
        {
            presenter.RequestBuySelected();
        }
    }

    public void OnSell()
    {
        if (presenter != null)
        {
            presenter.RequestSellSelected();
        }
    }

    public void OnLottery()
    {
        // Stage-1 placeholder for future lottery flow.
    }

    public void BindPresenter(ShopPagePresenter pagePresenter)
    {
        presenter = pagePresenter;
    }

    public void BindDetail(ShopCommodityViewModel viewModel)
    {
        if (detailIconImage != null)
        {
            detailIconImage.sprite = viewModel.Icon;
            detailIconImage.enabled = viewModel.Icon != null;
        }
        if (detailTitleText != null)
        {
            string itemName = string.IsNullOrWhiteSpace(viewModel.ItemName) ? $"Item({viewModel.ItemId})" : viewModel.ItemName;
            int titleCount = viewModel.TradeCount;
            if (viewModel.CanSell && !viewModel.CanBuy && viewModel.IsSellAll)
            {
                // SellOnly + IsSellAll：标题数量展示为“已拥有”，点击后卖出全部剩余存货。
                titleCount = viewModel.OwnedCount;
            }

            detailTitleText.text = $"{itemName}*{titleCount}（已拥有：{viewModel.OwnedCount}）";
        }
        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = string.IsNullOrWhiteSpace(viewModel.ItemDescription) ? "暂无描述" : viewModel.ItemDescription;
        }
        if (detailBuyPriceText != null)
        {
            int buyDisplayCount = viewModel.TradeCount;
            float buyDisplayPrice = viewModel.BuyPrice * buyDisplayCount;

            detailBuyPriceText.text = $"购买：{buyDisplayPrice:0.0}元";
        }
        if (detailSellPriceText != null)
        {
            int sellDisplayCount = viewModel.TradeCount;
            if (viewModel.CanSell && !viewModel.CanBuy && viewModel.IsSellAll)
            {
                sellDisplayCount = viewModel.OwnedCount;
            }

            float sellDisplayPrice = viewModel.SellPrice * sellDisplayCount;

            detailSellPriceText.text = $"出售：{sellDisplayPrice:0.0}元";
        }
        if (detailBuyButton != null)
        {
            detailBuyButton.gameObject.SetActive(viewModel.CanBuy);
            detailBuyButton.interactable = true;
        }
        if (detailSellButton != null)
        {
            detailSellButton.gameObject.SetActive(viewModel.CanSell);
            detailSellButton.interactable = true;
        }

        if (detailBuyPriceText != null)
        {
            detailBuyPriceText.gameObject.SetActive(viewModel.CanBuy);
        }
        if (detailSellPriceText != null)
        {
            detailSellPriceText.gameObject.SetActive(viewModel.CanSell);
        }
    }

    public void SetDetailEmptyState()
    {
        if (detailIconImage != null)
        {
            detailIconImage.sprite = null;
            detailIconImage.enabled = false;
        }
        if (detailTitleText != null)
        {
            detailTitleText.text = "未选择商品";
        }
        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = "请选择一个商品查看详情。";
        }
        if (detailBuyPriceText != null)
        {
            detailBuyPriceText.text = "购买：--";
        }
        if (detailSellPriceText != null)
        {
            detailSellPriceText.text = "出售：--";
        }
        if (detailBuyButton != null)
        {
            detailBuyButton.gameObject.SetActive(false);
            detailBuyButton.interactable = false;
        }
        if (detailSellButton != null)
        {
            detailSellButton.gameObject.SetActive(false);
            detailSellButton.interactable = false;
        }

        if (detailBuyPriceText != null)
        {
            detailBuyPriceText.gameObject.SetActive(false);
        }
        if (detailSellPriceText != null)
        {
            detailSellPriceText.gameObject.SetActive(false);
        }
    }
}
