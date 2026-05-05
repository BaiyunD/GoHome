/// <summary>
/// 战斗胜利且主结算（<see cref="BattleWinSettlement.ApplyAndComposeNarration"/>）已应用后的追加结算上下文。
/// </summary>
public sealed class BattleVictorySettlementContext
{
    public BattleVictorySettlementContext(
        BattleSettlementRewardSnapshot rewards,
        int baseVictoryAttackDelta,
        int baseVictoryDefenseDelta,
        int baseVictoryMaxHpDelta)
    {
        Rewards = rewards;
        BaseVictoryAttackDelta = baseVictoryAttackDelta;
        BaseVictoryDefenseDelta = baseVictoryDefenseDelta;
        BaseVictoryMaxHpDelta = baseVictoryMaxHpDelta;
    }

    public BattleSettlementRewardSnapshot Rewards { get; }

    public int BaseVictoryAttackDelta { get; }

    public int BaseVictoryDefenseDelta { get; }

    public int BaseVictoryMaxHpDelta { get; }
}
