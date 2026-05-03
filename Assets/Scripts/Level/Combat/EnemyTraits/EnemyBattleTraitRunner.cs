using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 执行 <see cref="EnemyBattleTraitAsset"/> 数值与战斗日志后缀合并。
/// </summary>
public static class EnemyBattleTraitRunner
{
    public static string MergeSuffixFragments(string before, string after)
    {
        if (string.IsNullOrEmpty(after))
        {
            return before ?? string.Empty;
        }

        if (string.IsNullOrEmpty(before))
        {
            return after;
        }

        return before + "。" + after;
    }

    public static IReadOnlyList<EnemyBattleTraitAsset> ResolveBattleTraitAssets()
    {
        EnemyRuntime current = EnemyStateManager.Instance != null ? EnemyStateManager.Instance.Current : null;
        EnemyData data = current != null ? current.RuntimeData : null;
        if (data == null)
        {
            return System.Array.Empty<EnemyBattleTraitAsset>();
        }

        return data.BattleTraits;
    }

    public static void RunAndMergeReceiveHitTraits(
        CombatActionResult result,
        CharacterRuntimeStats player,
        CharacterRuntimeStats enemy,
        string enemyBaseName,
        int damageDealtToEnemy)
    {
        if (result == null || player == null || enemy == null)
        {
            return;
        }

        IReadOnlyList<EnemyBattleTraitAsset> assets = ResolveBattleTraitAssets();
        if (assets == null || assets.Count == 0)
        {
            return;
        }

        string accumulated = string.Empty;
        for (int i = 0; i < assets.Count; i++)
        {
            EnemyBattleTraitAsset asset = assets[i];
            if (asset == null || asset.Trigger != EnemyBattleTraitTrigger.OnEnemyReceiveHit)
            {
                continue;
            }

            if (asset.OnlyWhenEnemyLostHpFromPlayerHit && damageDealtToEnemy <= 0)
            {
                continue;
            }

            string block = ExecuteTraitAndCompose(asset, player, enemy, enemyBaseName);
            if (!string.IsNullOrEmpty(block))
            {
                accumulated = MergeSuffixFragments(accumulated, block);
            }
        }

        if (string.IsNullOrEmpty(accumulated))
        {
            return;
        }

        TryMergeIntoLastAttackDefenderSuffix(result, accumulated);
    }

    public static void RunAndMergeAfterEnemyAttackTraits(
        CombatActionResult result,
        CharacterRuntimeStats player,
        CharacterRuntimeStats enemy,
        string enemyBaseName,
        int damageDealtToPlayer)
    {
        if (result == null || player == null || enemy == null)
        {
            return;
        }

        IReadOnlyList<EnemyBattleTraitAsset> assets = ResolveBattleTraitAssets();
        if (assets == null || assets.Count == 0)
        {
            return;
        }

        string accumulated = string.Empty;
        for (int i = 0; i < assets.Count; i++)
        {
            EnemyBattleTraitAsset asset = assets[i];
            if (asset == null || asset.Trigger != EnemyBattleTraitTrigger.OnEnemyAttackEnd)
            {
                continue;
            }

            if (asset.OnlyWhenPlayerLostHpFromEnemyHit && damageDealtToPlayer <= 0)
            {
                continue;
            }

            string block = ExecuteTraitAndCompose(asset, player, enemy, enemyBaseName);
            if (!string.IsNullOrEmpty(block))
            {
                accumulated = MergeSuffixFragments(accumulated, block);
            }
        }

        if (string.IsNullOrEmpty(accumulated))
        {
            return;
        }

        TryMergeIntoLastAttackAfterAttackSuffix(result, accumulated);
    }

    private static string ExecuteTraitAndCompose(
        EnemyBattleTraitAsset asset,
        CharacterRuntimeStats player,
        CharacterRuntimeStats enemy,
        string enemyBaseName)
    {
        var genericClauses = new List<string>();
        IReadOnlyList<EnemyTraitEffectLine> lines = asset.EffectLines;
        if (lines != null)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                EnemyTraitEffectLine line = lines[i];
                if (line == null || line.Kind == EnemyTraitEffectKind.None)
                {
                    continue;
                }

                if (!TryApplyEffectLine(line, player, enemy))
                {
                    continue;
                }

                if (EnemyTraitNarrationComposer.TryFormatEffectLine(line, enemyBaseName, out string clause)
                    && !string.IsNullOrEmpty(clause))
                {
                    genericClauses.Add(clause);
                }
            }
        }

        return EnemyTraitNarrationComposer.ComposeFullBlock(
            enemyBaseName,
            asset.TraitDisplayName,
            genericClauses,
            asset.SpecialClause,
            asset.SpecialClauseSlot,
            asset.FlavorClause);
    }

    private static bool TryApplyEffectLine(EnemyTraitEffectLine line, CharacterRuntimeStats player, CharacterRuntimeStats enemy)
    {
        if (line == null || player == null || enemy == null)
        {
            return false;
        }

        string customTrim = line.CustomNarrationFragment != null ? line.CustomNarrationFragment.Trim() : string.Empty;
        if (line.Kind == EnemyTraitEffectKind.None && !string.IsNullOrEmpty(customTrim))
        {
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

                player.ApplyDamage(line.IntValue);
                return true;
            case EnemyTraitEffectKind.PlayerAttackDelta:
                if (line.IntValue == 0)
                {
                    return false;
                }

                player.AddAttackModifier(line.IntValue);
                return true;
            case EnemyTraitEffectKind.PlayerDefenseDelta:
                if (line.IntValue == 0)
                {
                    return false;
                }

                player.AddDefenseModifier(line.IntValue);
                return true;
            case EnemyTraitEffectKind.PlayerCriticalRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                player.AddCriticalRateModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.PlayerCriticalDamageDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                player.AddCriticalDamageModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.PlayerBlockRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                player.AddBlockRateModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.PlayerDodgeRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                player.AddDodgeRateModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.PlayerEscapeRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                player.AddEscapeRateModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.EnemyHeal:
                if (line.IntValue <= 0)
                {
                    return false;
                }

                enemy.Heal(line.IntValue);
                return true;
            case EnemyTraitEffectKind.EnemyAttackDelta:
                if (line.IntValue == 0)
                {
                    return false;
                }

                enemy.AddAttackModifier(line.IntValue);
                return true;
            case EnemyTraitEffectKind.EnemyDefenseDelta:
                if (line.IntValue == 0)
                {
                    return false;
                }

                enemy.AddDefenseModifier(line.IntValue);
                return true;
            case EnemyTraitEffectKind.EnemyCriticalRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                enemy.AddCriticalRateModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.EnemyCriticalDamageDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                enemy.AddCriticalDamageModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.EnemyBlockRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                enemy.AddBlockRateModifier(line.FloatValue);
                return true;
            case EnemyTraitEffectKind.EnemyDodgeRateDelta:
                if (Mathf.Approximately(line.FloatValue, 0f))
                {
                    return false;
                }

                enemy.AddDodgeRateModifier(line.FloatValue);
                return true;
            default:
                Debug.LogWarning($"EnemyBattleTraitRunner: 未实现的枚举 {line.Kind}");
                return false;
        }
    }

    private static void TryMergeIntoLastAttackDefenderSuffix(CombatActionResult result, string suffixFragment)
    {
        if (string.IsNullOrEmpty(suffixFragment))
        {
            return;
        }

        int lastIdx = FindLastAttackLogIndex(result);
        if (lastIdx < 0)
        {
            return;
        }

        CombatSettlementLog last = result.SettlementLogs[lastIdx];
        BattleAttackEvent old = last.AttackEvent;
        string merged = MergeSuffixFragments(old.DefenderPhaseLogSuffix, suffixFragment);
        if (merged == old.DefenderPhaseLogSuffix)
        {
            return;
        }

        ReplaceAttackLogAt(result, lastIdx, new BattleAttackEvent(
            old.AttackerName,
            old.DefenderName,
            old.SkillLabel,
            old.Damage,
            old.IsCritical,
            old.IsBlocked,
            old.IsDodged,
            old.AttackerPhaseLogSuffix,
            merged,
            old.AfterAttackPhaseLogSuffix));
    }

    private static void TryMergeIntoLastAttackAfterAttackSuffix(CombatActionResult result, string suffixFragment)
    {
        if (string.IsNullOrEmpty(suffixFragment))
        {
            return;
        }

        int lastIdx = FindLastAttackLogIndex(result);
        if (lastIdx < 0)
        {
            return;
        }

        CombatSettlementLog last = result.SettlementLogs[lastIdx];
        BattleAttackEvent old = last.AttackEvent;
        string merged = MergeSuffixFragments(old.AfterAttackPhaseLogSuffix, suffixFragment);
        if (merged == old.AfterAttackPhaseLogSuffix)
        {
            return;
        }

        ReplaceAttackLogAt(result, lastIdx, new BattleAttackEvent(
            old.AttackerName,
            old.DefenderName,
            old.SkillLabel,
            old.Damage,
            old.IsCritical,
            old.IsBlocked,
            old.IsDodged,
            old.AttackerPhaseLogSuffix,
            old.DefenderPhaseLogSuffix,
            merged));
    }

    private static int FindLastAttackLogIndex(CombatActionResult result)
    {
        if (result?.SettlementLogs == null)
        {
            return -1;
        }

        for (int i = result.SettlementLogs.Count - 1; i >= 0; i--)
        {
            CombatSettlementLog log = result.SettlementLogs[i];
            if (log != null
                && log.LogType == CombatSettlementLogType.Attack
                && log.AttackEvent != null)
            {
                return i;
            }
        }

        return -1;
    }

    private static void ReplaceAttackLogAt(CombatActionResult result, int index, BattleAttackEvent newEvent)
    {
        result.SettlementLogs.RemoveAt(index);
        result.SettlementLogs.Insert(index, CombatSettlementLog.FromAttack(newEvent));
    }
}
