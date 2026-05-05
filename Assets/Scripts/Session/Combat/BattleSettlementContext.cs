public sealed class BattleSettlementContext
{
    public BattleSettlementContext(
        BattleResult result,
        string enemyEscapeDisplayName,
        BattleSettlementRewardSnapshot rewards)
    {
        Result = result;
        EnemyEscapeDisplayName = enemyEscapeDisplayName;
        Rewards = rewards ?? BattleSettlementRewardSnapshot.Empty();
    }

    public BattleResult Result { get; }

    /// <summary>敌逃等用于叙述的敌人名；可为 null。</summary>
    public string EnemyEscapeDisplayName { get; }

    public BattleSettlementRewardSnapshot Rewards { get; }
}
