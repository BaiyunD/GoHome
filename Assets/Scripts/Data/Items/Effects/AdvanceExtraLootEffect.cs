using UnityEngine;

[CreateAssetMenu(fileName = "AdvanceExtraLootEffect", menuName = "GoHome/Item Effects/Advance Extra Loot")]
public sealed class AdvanceExtraLootEffect : ItemEffectDefinition
{
    [Range(0f, 1f)]
    [SerializeField] private float chancePerLevel = 0.05f;
    [SerializeField] private bool requireMainResultEmpty;

    public void Configure(float chance, bool requireEmpty)
    {
        chancePerLevel = chance;
        requireMainResultEmpty = requireEmpty;
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

        if (requireMainResultEmpty && (context.MainResult == null || !context.MainResult.IsEmpty))
        {
            return false;
        }

        int level = Mathf.Max(0, context.TriggerItemLevel);
        if (level <= 0)
        {
            return false;
        }

        float chance = Mathf.Clamp01(level * Mathf.Max(0f, chancePerLevel));
        if (Random.value > chance)
        {
            return false;
        }

        result = RegionLootService.RollAndGrant(context.RegionCode, context.RegionLootTable);
        return true;
    }

}
