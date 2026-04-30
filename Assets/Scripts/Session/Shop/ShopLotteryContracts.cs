using System;

public enum ShopLotteryRewardKind
{
    Item = 0,
    Money = 1
}

public enum ShopLotteryDrawReason
{
    RolledThanks = 0,
    RolledTier = 1,
    InsufficientPoints = 2,
    InvalidPool = 3,
    GrantFailed = 4
}

[Serializable]
public struct ShopLotteryDrawResult
{
    public ShopLotteryDrawReason Reason;
    public string TierDisplayName;
    public ShopLotteryRewardKind RewardKind;
    public int ItemId;
    public int ItemCount;
    public string ItemDisplayName;
    public float MoneyAmount;
    public string DetailMessage;
}
