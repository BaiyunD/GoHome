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
        result.Effects.Add(new CombatActionEffect(CombatActionEffectType.DamageEnemy, resolution.Damage));
        result.SettlementLogs.Add(
            CombatSettlementLog.FromAttack(new BattleAttackEvent(
                "你",
                context.EnemyName,
                "普攻",
                resolution.Damage,
                resolution.IsCritical,
                resolution.IsBlocked,
                resolution.IsDodged
            ))
        );
        return result;
    }
}
