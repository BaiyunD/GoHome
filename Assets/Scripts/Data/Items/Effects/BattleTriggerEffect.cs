using UnityEngine;

public enum BattleTriggerEffectMode
{
    LifestealAfterAttack = 0,
    ReflectAfterReceiveHit = 1,
    BonusDamageAfterAttack = 2,
    HealAfterReceiveHit = 3,
    EnemyDefenseReduceBattleStart = 4,
    EnemyEscapeReduceBattleStart = 5,
    BonusMaterialOnBattleWin = 6
}

[CreateAssetMenu(fileName = "BattleTriggerEffect", menuName = "GoHome/Item Effects/Battle Trigger")]
public sealed class BattleTriggerEffect : ItemEffectDefinition
{
    [SerializeField] private BattleTriggerEffectMode mode = BattleTriggerEffectMode.LifestealAfterAttack;
    [Range(0f, 1f)]
    [SerializeField] private float chancePerLevel = 0.02f;
    [SerializeField] private float factorPerLevel = 0.1f;
    [SerializeField] private int fixedReflectDamage = 2;
    [SerializeField] private int[] bonusMaterialItemIds = new int[0];
    [SerializeField] private int bonusMaterialCount = 1;

    public override void OnBattleHookItemEffect(BattleEffectContext context, int level)
    {
        if (context == null || level <= 0)
        {
            return;
        }

        float chance = Mathf.Clamp01(Mathf.Max(0f, chancePerLevel) * level);
        if (chance > 0f && Random.value > chance)
        {
            return;
        }

        switch (mode)
        {
            case BattleTriggerEffectMode.LifestealAfterAttack:
                ApplyLifesteal(context, level);
                break;
            case BattleTriggerEffectMode.ReflectAfterReceiveHit:
                ApplyReflect(context, level);
                break;
            case BattleTriggerEffectMode.BonusDamageAfterAttack:
                ApplyBonusDamage(context, level);
                break;
            case BattleTriggerEffectMode.HealAfterReceiveHit:
                ApplyHealAfterReceive(context, level);
                break;
            case BattleTriggerEffectMode.EnemyDefenseReduceBattleStart:
                ApplyEnemyDefenseReduce(context, level);
                break;
            case BattleTriggerEffectMode.EnemyEscapeReduceBattleStart:
                ApplyEnemyEscapeReduce(context, level);
                break;
            case BattleTriggerEffectMode.BonusMaterialOnBattleWin:
                ApplyBonusMaterialOnWin(context, level);
                break;
        }
    }

    private void ApplyLifesteal(BattleEffectContext context, int level)
    {
        if (context.Hook != BattleEffectHook.AfterAttack || context.Attacker == null || context.Owner != BattleEffectOwner.Player)
        {
            return;
        }

        int heal = Mathf.Max(0, Mathf.RoundToInt(context.ComputedDamage * Mathf.Max(0f, factorPerLevel) * level));
        context.Attacker.Heal(heal);
    }

    private void ApplyReflect(BattleEffectContext context, int level)
    {
        if (context.Hook != BattleEffectHook.AfterReceiveHit || context.Owner != BattleEffectOwner.Player || context.Attacker == null || context.Defender == null)
        {
            return;
        }

        int mitigationHeal = Mathf.Max(0, Mathf.RoundToInt(context.Defender.Defense * Mathf.Max(0f, factorPerLevel) * level));
        context.Defender.Heal(mitigationHeal);
        int reflectDamage = Mathf.Max(0, fixedReflectDamage * level);
        context.Attacker.ApplyDamage(reflectDamage);
    }

    private void ApplyBonusDamage(BattleEffectContext context, int level)
    {
        if (context.Hook != BattleEffectHook.AfterAttack || context.Owner != BattleEffectOwner.Player || context.Defender == null)
        {
            return;
        }

        int bonusDamage = Mathf.Max(0, Mathf.RoundToInt(context.ComputedDamage * Mathf.Max(0f, factorPerLevel) * level));
        context.Defender.ApplyDamage(bonusDamage);
    }

    private void ApplyHealAfterReceive(BattleEffectContext context, int level)
    {
        if (context.Hook != BattleEffectHook.AfterReceiveHit || context.Owner != BattleEffectOwner.Player || context.Defender == null)
        {
            return;
        }

        int healAmount = Mathf.Max(0, Mathf.RoundToInt(context.Defender.MaxHp * Mathf.Max(0f, factorPerLevel) * level));
        context.Defender.Heal(healAmount);
    }

    private void ApplyEnemyDefenseReduce(BattleEffectContext context, int level)
    {
        if (context.Hook != BattleEffectHook.BattleStart || context.Owner != BattleEffectOwner.Player || context.Defender == null)
        {
            return;
        }

        int reduce = Mathf.Max(0, Mathf.RoundToInt(context.Defender.Defense * Mathf.Max(0f, factorPerLevel) * level));
        context.Defender.AddDefenseModifier(-reduce);
    }

    private void ApplyEnemyEscapeReduce(BattleEffectContext context, int level)
    {
        if (context.Hook == BattleEffectHook.BattleStart && context.Owner == BattleEffectOwner.Player && context.Defender != null)
        {
            context.Defender.AddEscapeRateModifier(-Mathf.Max(0f, factorPerLevel) * level * 100f);
            return;
        }

        if (context.Hook != BattleEffectHook.TryFlee || context.Owner != BattleEffectOwner.Enemy)
        {
            return;
        }

        context.FleeRate = Mathf.Max(0f, context.FleeRate - Mathf.Max(0f, factorPerLevel) * level * 100f);
    }

    private void ApplyBonusMaterialOnWin(BattleEffectContext context, int level)
    {
        if (context.Hook != BattleEffectHook.BattleEnd
            || context.Owner != BattleEffectOwner.Player
            || context.EndResult != BattleResult.Win
            || InventoryManager.Instance == null
            || bonusMaterialItemIds == null
            || bonusMaterialItemIds.Length == 0)
        {
            return;
        }

        int idx = Random.Range(0, bonusMaterialItemIds.Length);
        int itemId = bonusMaterialItemIds[idx];
        int count = Mathf.Max(1, bonusMaterialCount) * level;
        InventoryManager.Instance.AddItem(itemId, count);
    }
}
