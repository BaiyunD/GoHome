using UnityEngine;

[CreateAssetMenu(
    fileName = "MagnifierExtraLootEffect",
    menuName = "GoHome/Item Effects/Specific/Magnifier Extra Loot")]
public sealed class MagnifierExtraLootEffect : ItemEffectDefinition
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
