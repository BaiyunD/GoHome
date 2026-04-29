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
}
