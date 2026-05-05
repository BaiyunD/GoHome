using UnityEngine;

public abstract class ItemEffectDefinition : ScriptableObject
{
    [SerializeField] private string effectId;
    [TextArea(2, 4)]
    [SerializeField] private string effectDescription;
    [SerializeField] private int displayPriority = 100;

    public string EffectId => effectId;
    public string EffectDescription => effectDescription;
    public int DisplayPriority => displayPriority;

    public virtual void OnPassiveItemEffect(PassiveAccumulator accumulator, int level)
    {
    }

    public virtual void OnRestItemEffect(RestContext context, int level)
    {
    }

    public virtual bool OnAdvanceSupplyItemEffect(
        AdvanceSupplyEffectContext context,
        out RegionLootService.LootRollResult result)
    {
        result = null;
        return false;
    }

    public virtual bool OnUseItemEffect(ItemBase item, int level, out string resultText)
    {
        resultText = string.Empty;
        return false;
    }

    public virtual void OnBattleHookItemEffect(BattleEffectContext context, int level)
    {
    }

    /// <summary>
    /// 战斗胜利且主结算已完成后调用；可再次改写背包/金钱/属性。
    /// 返回追加到胜利弹窗的叙述片段（不要前置换行）。
    /// </summary>
    public virtual string OnBattleVictorySettlement(BattleVictorySettlementContext context, int level)
    {
        return string.Empty;
    }
}
