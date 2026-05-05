public static class BattleSettlementRelay
{
    public static string Dispatch(
        BattleSettlementContext context,
        int baseVictoryAttackDelta,
        int baseVictoryDefenseDelta,
        int baseVictoryMaxHpDelta)
    {
        if (context == null)
        {
            return string.Empty;
        }

        switch (context.Result)
        {
            case BattleResult.Win:
                return BattleWinSettlement.ApplyAndComposeNarration(
                    context.Rewards,
                    baseVictoryAttackDelta,
                    baseVictoryDefenseDelta,
                    baseVictoryMaxHpDelta);
            case BattleResult.Lose:
                return BattleLoseSettlement.GetNarration();
            case BattleResult.Escape:
            case BattleResult.EnemyEscape:
                return BattleFleeSettlement.GetNarration(context.Result, context.EnemyEscapeDisplayName);
            default:
                return string.Empty;
        }
    }
}
