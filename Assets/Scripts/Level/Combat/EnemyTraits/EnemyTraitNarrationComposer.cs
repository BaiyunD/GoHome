using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 将敌人战斗特性的通用效果句拼为规范括号文案。
/// </summary>
public static class EnemyTraitNarrationComposer
{
    public static string ComposeFullBlock(
        string enemyBaseName,
        string traitDisplayName,
        List<string> genericClauses,
        string flavorClause)
    {
        if (string.IsNullOrEmpty(enemyBaseName) || string.IsNullOrEmpty(traitDisplayName))
        {
            return string.Empty;
        }

        string enemy = enemyBaseName.Trim();
        string trait = traitDisplayName.Trim();
        string flavor = (flavorClause ?? string.Empty).Trim();

        string genericsJoined = JoinNonEmpty("，", genericClauses);
        string body = BuildBody(genericsJoined, flavor);
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        return $"【{enemy}使用[{trait}]，{body}。】";
    }

    public static string FormatSignedIntStatClause(string subject, string statLabel, int delta)
    {
        if (delta == 0)
        {
            return string.Empty;
        }

        string verb = delta < 0 ? "下降" : "增加";
        int abs = Mathf.Abs(delta);
        return $"{subject}{statLabel}{verb}{abs}点";
    }

    public static string FormatSignedRateClause(string subject, string statLabel, float deltaPercentPoints)
    {
        if (Mathf.Approximately(deltaPercentPoints, 0f))
        {
            return string.Empty;
        }

        string verb = deltaPercentPoints < 0f ? "下降" : "上升";
        float abs = Mathf.Abs(deltaPercentPoints);
        string num = abs % 1f < 0.001f ? abs.ToString("0") : abs.ToString("0.#");
        return $"{subject}{statLabel}{verb}{num}%";
    }

    public static bool TryFormatEffectLine(
        EnemyTraitEffectLine line,
        string enemyName,
        out string clause)
    {
        clause = string.Empty;
        if (line == null)
        {
            return false;
        }

        string custom = line.CustomNarrationFragment != null ? line.CustomNarrationFragment.Trim() : string.Empty;
        if (!string.IsNullOrEmpty(custom))
        {
            clause = custom;
            return true;
        }

        switch (line.Kind)
        {
            case EnemyTraitEffectKind.None:
                return false;
            case EnemyTraitEffectKind.PlayerFlatDamage:
                if (line.IntValue <= 0)
                {
                    return false;
                }

                clause = $"你受到{line.IntValue}点伤害";
                return true;
            case EnemyTraitEffectKind.PlayerAttackDelta:
                clause = FormatSignedIntStatClause("你", "攻击", line.IntValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.PlayerDefenseDelta:
                clause = FormatSignedIntStatClause("你", "防御", line.IntValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.PlayerCriticalRateDelta:
                clause = FormatSignedRateClause("你", "暴击", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.PlayerCriticalDamageDelta:
                clause = FormatSignedRateClause("你", "暴击伤害", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.PlayerBlockRateDelta:
                clause = FormatSignedRateClause("你", "格挡", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.PlayerDodgeRateDelta:
                clause = FormatSignedRateClause("你", "闪避", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.PlayerEscapeRateDelta:
                clause = FormatSignedRateClause("你", "逃跑", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.EnemyHeal:
                if (line.IntValue <= 0)
                {
                    return false;
                }

                clause = $"{enemyName}恢复{line.IntValue}点生命值";
                return true;
            case EnemyTraitEffectKind.EnemyAttackDelta:
                clause = FormatSignedIntStatClause(enemyName, "攻击", line.IntValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.EnemyDefenseDelta:
                clause = FormatSignedIntStatClause(enemyName, "防御", line.IntValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.EnemyCriticalRateDelta:
                clause = FormatSignedRateClause(enemyName, "暴击", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.EnemyCriticalDamageDelta:
                clause = FormatSignedRateClause(enemyName, "暴击伤害", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.EnemyBlockRateDelta:
                clause = FormatSignedRateClause(enemyName, "格挡", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            case EnemyTraitEffectKind.EnemyDodgeRateDelta:
                clause = FormatSignedRateClause(enemyName, "闪避", line.FloatValue);
                return !string.IsNullOrEmpty(clause);
            default:
                Debug.LogWarning($"EnemyTraitNarrationComposer: 未覆盖的枚举 {line.Kind}，请补模板。");
                return false;
        }
    }

    private static string BuildBody(string genericsJoined, string flavor)
    {
        bool hasGen = !string.IsNullOrEmpty(genericsJoined);
        bool hasFl = !string.IsNullOrEmpty(flavor);

        if (!hasGen && !hasFl)
        {
            return string.Empty;
        }

        return JoinNonEmpty("，", genericsJoined, flavor);
    }

    private static string JoinNonEmpty(string separator, params string[] parts)
    {
        if (parts == null || parts.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < parts.Length; i++)
        {
            string p = parts[i];
            if (string.IsNullOrEmpty(p))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append(separator);
            }

            sb.Append(p);
        }

        return sb.ToString();
    }

    private static string JoinNonEmpty(string separator, List<string> items)
    {
        if (items == null || items.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < items.Count; i++)
        {
            string p = items[i];
            if (string.IsNullOrEmpty(p))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append(separator);
            }

            sb.Append(p);
        }

        return sb.ToString();
    }
}
