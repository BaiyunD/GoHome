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

    public static string BuildMessage(ShopTradeResult result)
    {
        if (result.Success)
        {
            if (result.Operation == ShopTradeOperation.Buy)
            {
                return $"您已购买【{result.ItemName}】*{result.TradeCount}！获得{result.DeltaPoints}积分";
            }

            if (result.Operation == ShopTradeOperation.Sell)
            {
                return $"您已获得{result.TotalPrice:0.0}元！获得{result.DeltaPoints}积分";
            }

            return "交易成功。";
        }

        if (result.Operation == ShopTradeOperation.Buy)
        {
            return "当前金钱不够！";
        }

        if (result.Operation == ShopTradeOperation.Sell)
        {
            return "当前物品不够！";
        }

        return "交易失败。";
    }
}
