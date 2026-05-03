/// <summary>
/// 敌人战斗特性触发时机（由 <see cref="EnemyBattleTraitAsset"/> 配置）。
/// </summary>
public enum EnemyBattleTraitTrigger
{
    /// <summary>玩家对敌伤害结算之后、敌行动之前；日志合并进玩家攻击事件的 Defender 后缀。</summary>
    OnEnemyReceiveHit = 0,

    /// <summary>敌攻伤害与玩家 AfterReceiveHit 物品钩子之后、敌回合结算展示之前；日志合并进敌攻事件的 AfterAttack 后缀。</summary>
    OnEnemyAttackEnd = 1
}
