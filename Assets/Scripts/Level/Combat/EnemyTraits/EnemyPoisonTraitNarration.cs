/// <summary>
/// 中毒类 Special 特性文案模板（与通用 <see cref="EnemyTraitNarrationComposer"/> 分离）。
/// </summary>
public static class EnemyPoisonTraitNarration
{
    private const string PoisonRoundEndTemplate = "回合结束失去{0}点生命值（{1}层）";

    /// <summary>施加时写入括号正文（本段失去量、展示层数）。</summary>
    public static string FormatPoisonRoundEndClause(int damageThisRound, int layers)
    {
        if (damageThisRound <= 0 || layers <= 0)
        {
            return string.Empty;
        }

        return string.Format(PoisonRoundEndTemplate, damageThisRound, layers);
    }

    /// <summary>中毒施加：基数 x、展示层数；句子中失去量为 x×层数。</summary>
    public static string FormatPoisonInflictClause(int basePerLayer, int displayLayers)
    {
        if (basePerLayer <= 0 || displayLayers <= 0)
        {
            return string.Empty;
        }

        return FormatPoisonRoundEndClause(basePerLayer * displayLayers, displayLayers);
    }
}
