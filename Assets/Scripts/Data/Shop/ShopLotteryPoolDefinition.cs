using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopLotteryRewardOptionData
{
    [SerializeField] private ShopLotteryRewardKind kind = ShopLotteryRewardKind.Item;
    [SerializeField] private int itemId = 1;
    [SerializeField] private int itemCount = 1;
    [SerializeField] private float moneyAmount = 1f;

    public ShopLotteryRewardKind Kind => kind;
    public int ItemId => itemId;
    public int ItemCount => itemCount;
    public float MoneyAmount => moneyAmount;
}

[Serializable]
public class ShopLotteryTierData
{
    [SerializeField] private string displayName = "三等奖";
    [SerializeField] private float weightPercent = 10f;
    [SerializeField] private List<ShopLotteryRewardOptionData> rewardOptions = new List<ShopLotteryRewardOptionData>();

    public string DisplayName => displayName;
    public float WeightPercent => weightPercent;
    public IReadOnlyList<ShopLotteryRewardOptionData> RewardOptions => rewardOptions;
}

[CreateAssetMenu(fileName = "ShopLotteryPool", menuName = "GoHome/Shop/Lottery Pool")]
public class ShopLotteryPoolDefinition : ScriptableObject
{
    public const string ReservedThanksDisplayName = "谢谢惠顾";

    [SerializeField] private List<ShopLotteryTierData> tiers = new List<ShopLotteryTierData>();

    public IReadOnlyList<ShopLotteryTierData> Tiers => tiers;

    public float SumTierWeights()
    {
        float sum = 0f;
        if (tiers == null)
        {
            return 0f;
        }

        for (int i = 0; i < tiers.Count; i++)
        {
            ShopLotteryTierData tier = tiers[i];
            if (tier == null)
            {
                continue;
            }

            float w = tier.WeightPercent;
            if (w > 0f)
            {
                sum += w;
            }
        }

        return sum;
    }
}
