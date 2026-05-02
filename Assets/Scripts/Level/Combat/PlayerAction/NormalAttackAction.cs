public sealed class NormalAttackAction : IPlayerAction
{
    public CombatActionResult Execute(PlayerActionContext context)
    {
        CombatActionResult result = new CombatActionResult();
        if (context == null || context.Player == null || context.Enemy == null)
        {
            return result;
        }

        NormalAttackResolution resolution = NormalAttackResolver.Resolve(
            context.Player,
            context.Enemy,
            context.DamageBoostMultiplier,
            context.DamageReductionMultiplier
        );
        int damage = resolution.Damage;
        BattleItemHookRunner.RunPlayerBeforeAttack(
            context.Player,
            context.Enemy,
            ref damage,
            out string attackerPhaseLog,
            out string defenderPhaseLog);
        result.Effects.Add(new CombatActionEffect(CombatActionEffectType.DamageEnemy, damage));
        result.SettlementLogs.Add(
            CombatSettlementLog.FromAttack(new BattleAttackEvent(
                "你",
                context.EnemyName,
                "普攻",
                damage,
                resolution.IsCritical,
                resolution.IsBlocked,
                resolution.IsDodged,
                attackerPhaseLog,
                defenderPhaseLog
            ))
        );
        return result;
    }
}
