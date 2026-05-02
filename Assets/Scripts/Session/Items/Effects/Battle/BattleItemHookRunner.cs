using UnityEngine;

public static class BattleItemHookRunner
{
    public static void RunPlayerBeforeAttack(
        CharacterRuntimeStats player,
        CharacterRuntimeStats enemy,
        ref int damage,
        out string attackerPhaseLogSuffix,
        out string defenderPhaseLogSuffix)
    {
        BattleEffectContext context = new BattleEffectContext
        {
            Hook = BattleEffectHook.BeforeAttack,
            Owner = BattleEffectOwner.Player,
            Attacker = player,
            Defender = enemy,
            ComputedDamage = damage,
            FinalDamage = damage,
            TurnIndex = 0
        };

        ItemEffectDispatcher.OnBattleHookItemEffect(context);
        damage = Mathf.Max(0, context.FinalDamage);
        attackerPhaseLogSuffix = context.BuildAttackerPhaseLogSuffix();
        defenderPhaseLogSuffix = context.BuildDefenderPhaseLogSuffix();
    }

    public static void RunPlayerBeforeReceiveHit(
        CharacterRuntimeStats enemyAttacker,
        CharacterRuntimeStats playerDefender,
        ref int damage,
        out string attackerPhaseLogSuffix,
        out string defenderPhaseLogSuffix)
    {
        BattleEffectContext context = new BattleEffectContext
        {
            Hook = BattleEffectHook.BeforeReceiveHit,
            Owner = BattleEffectOwner.Player,
            Attacker = enemyAttacker,
            Defender = playerDefender,
            ComputedDamage = damage,
            FinalDamage = damage,
            TurnIndex = 0
        };

        ItemEffectDispatcher.OnBattleHookItemEffect(context);
        damage = Mathf.Max(0, context.FinalDamage);
        attackerPhaseLogSuffix = context.BuildAttackerPhaseLogSuffix();
        defenderPhaseLogSuffix = context.BuildDefenderPhaseLogSuffix();
    }

    public static void RunPlayerAfterAttack(
        CharacterRuntimeStats player,
        CharacterRuntimeStats enemy,
        int finalDamageDealtToEnemy,
        out string afterAttackPhaseLogSuffix)
    {
        BattleEffectContext context = new BattleEffectContext
        {
            Hook = BattleEffectHook.AfterAttack,
            Owner = BattleEffectOwner.Player,
            Attacker = player,
            Defender = enemy,
            ComputedDamage = finalDamageDealtToEnemy,
            FinalDamage = finalDamageDealtToEnemy,
            TurnIndex = 0
        };

        ItemEffectDispatcher.OnBattleHookItemEffect(context);
        afterAttackPhaseLogSuffix = context.BuildAfterAttackPhaseLogSuffix();
    }
}
