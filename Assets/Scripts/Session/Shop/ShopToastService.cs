using UnityEngine;

public sealed class ShopToastService : MonoBehaviour
{
    public static ShopToastService Instance
    {
        get; private set;
    }

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

    public void NotifyTradeResult(ShopTradeResult result)
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.ShowResultToast(BuildMessage(result));
    }

    private static string BuildMessage(ShopTradeResult result)
    {
        if (result.Success)
        {
            if (result.IsBuyAndSellTrade && result.Operation == ShopTradeOperation.Buy)
            {
                return $"您已购买【{result.ItemName}】*{result.TradeCount}！获得{result.DeltaPoints}积分";
            }

            if (result.IsBuyAndSellTrade && result.Operation == ShopTradeOperation.Sell)
            {
                return $"您已获得{result.TotalPrice:0.0}元！获得{result.DeltaPoints}积分";
            }

            return "交易成功。";
        }

        if (result.IsBuyAndSellTrade && result.Operation == ShopTradeOperation.Buy &&
            result.ReasonCode == ShopTradeReasonCode.InsufficientCash)
        {
            return "当前金钱不够！";
        }

        if (result.IsBuyAndSellTrade && result.Operation == ShopTradeOperation.Sell &&
            result.ReasonCode == ShopTradeReasonCode.InsufficientInventory)
        {
            return "当前物品不够！";
        }

        switch (result.ReasonCode)
        {
            case ShopTradeReasonCode.FeatureDisabled:
                return "商店功能未开启。";
            case ShopTradeReasonCode.InvalidRequest:
                return "交易参数无效。";
            case ShopTradeReasonCode.InsufficientCash:
                return "金钱不足，无法购买。";
            case ShopTradeReasonCode.InsufficientInventory:
                return "拥有数量不足，无法售出。";
            case ShopTradeReasonCode.CommodityMissing:
                return "商品不存在或已下架。";
            default:
                return "交易失败。";
        }
    }
}
