using UnityEngine;

public sealed class NormalAttackAction : IPlayerAction
{
    public CombatActionResult Execute(PlayerActionContext context)
    {
        CombatActionResult result = new CombatActionResult();
        if (context == null || context.Player == null || context.Enemy == null)
        {
            return result;
        }

        int damage = Mathf.Max(0, context.Player.Attack - context.Enemy.Defense);
        result.Effects.Add(new CombatActionEffect(CombatActionEffectType.DamageEnemy, damage));
        result.SettlementLogs.Add(
            CombatSettlementLog.FromAttack(new BattleAttackEvent("你", context.EnemyName, "普攻", damage))
        );
        return result;
    }
}
