using UnityEngine;

[CreateAssetMenu(fileName = "DaggerBloodthirstEffect", menuName = "GoHome/Item Effects/Specific/Dagger Bloodthirst")]
public sealed class DaggerBloodthirstEffect : ItemEffectDefinition
{
    [SerializeField] private float chancePerLevelPercent = 2f;
    [SerializeField] private float healRatioOnTriggerPercent = 21f;

    public void Configure(float chancePerLevelValue, float healRatioOnTriggerValue)
    {
        chancePerLevelPercent = chancePerLevelValue;
        healRatioOnTriggerPercent = healRatioOnTriggerValue;
    }

    public override void OnBattleHookItemEffect(BattleEffectContext context, int level)
    {
        if (context == null
            || level <= 0
            || context.Hook != BattleEffectHook.AfterAttack
            || context.Owner != BattleEffectOwner.Player
            || context.Attacker == null)
        {
            return;
        }

        float triggerChance = Mathf.Max(0f, chancePerLevelPercent) * level / 100f;
        if (triggerChance <= 0f || Random.value >= triggerChance)
        {
            return;
        }

        int dealtDamage = Mathf.Max(0, context.FinalDamage > 0 ? context.FinalDamage : context.ComputedDamage);
        if (dealtDamage <= 0)
        {
            return;
        }

        float healRatio = Mathf.Max(0f, healRatioOnTriggerPercent) / 100f;
        int healAmount = Mathf.Max(0, Mathf.RoundToInt(dealtDamage * healRatio));
        if (healAmount <= 0)
        {
            return;
        }

        context.Attacker.Heal(healAmount);
        context.AppendAfterAttackPhaseLog($"【嗜血】恢复{healAmount}点生命值");
    }

}
