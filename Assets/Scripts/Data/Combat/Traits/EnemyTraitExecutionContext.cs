/// <summary>
/// 特性执行所处战斗钩子阶段（由 <see cref="EnemyBattleTraitRunner"/> 传入）。
/// </summary>
public enum EnemyBattleTraitHookPhase
{
    OnEnemyReceiveHit,
    OnEnemyAttackEnd,
}

/// <summary>
/// <see cref="EnemyBattleTraitAsset.TryExecuteAndCompose"/> 的输入上下文。
/// </summary>
public struct EnemyTraitExecutionContext
{
    public CharacterRuntimeStats Player;
    public CharacterRuntimeStats Enemy;
    public string EnemyBaseName;
    public EnemyBattleTraitHookPhase Hook;
}
