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

    public void NotifyLotteryResult(ShopLotteryDrawResult result)
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.ShowResultToast(BuildLotteryMessage(result));
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
                return $"您已获得{result.TotalPrice:0.00}元！获得{result.DeltaPoints}积分";
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

    public static string BuildLotteryMessage(ShopLotteryDrawResult result)
    {
        switch (result.Reason)
        {
            case ShopLotteryDrawReason.InsufficientPoints:
                return "积分不足！";
            case ShopLotteryDrawReason.RolledThanks:
                return "什么都没中哦~谢谢惠顾~";
            case ShopLotteryDrawReason.RolledTier:
                if (result.RewardKind == ShopLotteryRewardKind.Money)
                {
                    string tierName = string.IsNullOrWhiteSpace(result.TierDisplayName) ? "奖" : result.TierDisplayName.Trim();
                    return $"恭喜您中了{tierName}，获得{result.MoneyAmount:0.00}元";
                }

                string tierNameItem = string.IsNullOrWhiteSpace(result.TierDisplayName) ? "奖" : result.TierDisplayName.Trim();
                string itemName = string.IsNullOrWhiteSpace(result.ItemDisplayName) ? $"Item({result.ItemId})" : result.ItemDisplayName.Trim();
                return $"恭喜您中了{tierNameItem}，获得{result.ItemCount}个{itemName}";
            case ShopLotteryDrawReason.InvalidPool:
                return string.IsNullOrWhiteSpace(result.DetailMessage) ? "奖池配置无效，请检查各档位概率与奖励。" : result.DetailMessage.Trim();
            case ShopLotteryDrawReason.GrantFailed:
                return string.IsNullOrWhiteSpace(result.DetailMessage) ? "奖励发放失败，积分已退回。" : result.DetailMessage.Trim();
            default:
                return "抽奖失败。";
        }
    }
}
