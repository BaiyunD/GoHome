using UnityEngine;

[CreateAssetMenu(
    fileName = "XiaoyueBraceletExtraLootEffect",
    menuName = "GoHome/Item Effects/Specific/Xiaoyue Bracelet Extra Loot")]
public sealed class XiaoyueBraceletExtraLootEffect : ItemEffectDefinition
{
    [SerializeField] private float chancePerLevelPercent = 5f;

    public void Configure(float chancePerLevelPercentValue)
    {
        chancePerLevelPercent = chancePerLevelPercentValue;
    }

    public override bool OnAdvanceSupplyItemEffect(
        AdvanceSupplyEffectContext context,
        out RegionLootService.LootRollResult result)
    {
        result = null;
        if (context == null || context.RegionLootTable == null)
        {
            return false;
        }

        if (context.MainResult == null || !context.MainResult.IsEmpty)
        {
            return false;
        }

        int level = Mathf.Max(0, context.TriggerItemLevel);
        if (level <= 0)
        {
            return false;
        }

        float chance = Mathf.Max(0f, chancePerLevelPercent) * level / 100f;
        if (Random.value > chance)
        {
            return false;
        }

        result = RegionLootService.RollAndGrant(context.RegionCode, context.RegionLootTable);
        return true;
    }

}
