using UnityEngine;

/// <summary>
/// 幸运石：战斗胜利主结算后按概率再结算一次「基础三选一 + commonRewards」，不重复 extra；
/// 叙述包在【幸运石：…】内。敌人「胜利补充句」在敌人资产上配置，由 <see cref="BattleManager"/> 拼在幸运石块之后。
/// </summary>
[CreateAssetMenu(fileName = "LuckyStoneVictoryDuplicateEffect", menuName = "GoHome/Item Effects/Specific/Lucky Stone Victory Duplicate")]
public sealed class LuckyStoneVictoryDuplicateEffect : ItemEffectDefinition
{
    [Tooltip("每持有 1 件叠加一层概率；总触发率 = min(100, 本值 × 背包数量)。Inspector 为 0–100。")]
    [Range(0f, 100f)]
    [SerializeField] private float triggerChancePercentPerLevel = 2f;

    public override string OnBattleVictorySettlement(BattleVictorySettlementContext context, int level)
    {
        if (context == null || level <= 0 || context.Rewards == null)
        {
            return string.Empty;
        }

        float threshold = Mathf.Min(100f, Mathf.Max(0f, triggerChancePercentPerLevel) * level);
        if (threshold <= 0f)
        {
            return string.Empty;
        }

        if (Random.Range(0f, 100f) >= threshold)
        {
            return string.Empty;
        }

        string inner = BattleWinSettlement.ApplyBonusBaseAndCommonOnly(
            context.Rewards,
            context.BaseVictoryAttackDelta,
            context.BaseVictoryDefenseDelta,
            context.BaseVictoryMaxHpDelta);

        return $"【幸运石：{inner}】";
    }
}
