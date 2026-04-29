using UnityEngine;

[CreateAssetMenu(fileName = "ThornsReflectEffect", menuName = "GoHome/Item Effects/Specific/Thorns Reflect")]
public sealed class ThornsReflectEffect : ItemEffectDefinition
{
    [SerializeField] private float triggerChancePerLevelPercent = 2f;
    [SerializeField] private float defenseMitigationPercent = 100f;

    public void Configure(float triggerChancePerLevelValue, float defenseMitigationPercentValue)
    {
        triggerChancePerLevelPercent = triggerChancePerLevelValue;
        defenseMitigationPercent = defenseMitigationPercentValue;
    }

    public override void OnBattleHookItemEffect(BattleEffectContext context, int level)
    {
        if (context == null
            || level <= 0
            || context.Hook != BattleEffectHook.AfterReceiveHit
            || context.Owner != BattleEffectOwner.Player
            || context.Attacker == null
            || context.Defender == null)
        {
            return;
        }

        float triggerChance = Mathf.Max(0f, triggerChancePerLevelPercent) * level / 100f;
        if (triggerChance <= 0f || Random.value >= triggerChance)
        {
            return;
        }

        float mitigationRatio = Mathf.Max(0f, defenseMitigationPercent) / 100f;
        int mitigateAmount = Mathf.Max(0, Mathf.RoundToInt(context.Defender.Defense * mitigationRatio));
        if (mitigateAmount > 0)
        {
            context.Defender.Heal(mitigateAmount);
        }

        if (mitigateAmount > 0)
        {
            context.Attacker.ApplyDamage(mitigateAmount);
        }
    }

}
