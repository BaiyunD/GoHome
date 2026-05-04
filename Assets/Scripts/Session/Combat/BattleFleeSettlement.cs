public static class BattleFleeSettlement
{
    public static string GetNarration(BattleResult result, string enemyEscapeDisplayName)
    {
        switch (result)
        {
            case BattleResult.Escape:
                return "你成功逃离了战斗。";
            case BattleResult.EnemyEscape:
                if (!string.IsNullOrWhiteSpace(enemyEscapeDisplayName))
                {
                    return $"{enemyEscapeDisplayName}逃跑了~";
                }

                return "敌人逃跑了~";
            default:
                return string.Empty;
        }
    }
}
