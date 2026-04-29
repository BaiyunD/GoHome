using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolService : MonoBehaviour
{
    public static EnemyPoolService Instance { get; private set; }

    [Header("敌人资产目录（Resources 下）")]
    [SerializeField] private string enemyResourcesFolder = "CharacterData/EnemyData";
    [Header("地区敌人池配置")]
    [SerializeField] private RegionEnemyTable regionEnemyTable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("EnemyPoolService.Awake -> 检测到重复 EnemyPoolService，请确保场景中只挂载一个。");
            return;
        }

        Instance = this;
        if (regionEnemyTable == null)
        {
            regionEnemyTable = Resources.Load<RegionEnemyTable>("RegionTable/Enemy/RegionEnemyTable_Main");
        }
    }

    public EnemyData GetRandomEnemyByRegionCode(string regionCode)
    {
        EventCondition.ValidateRegionCodeOrThrow(regionCode, "EnemyPoolService.regionCode");
        RegionEnemyPool pool = GetPoolOrThrow(regionCode);
        List<WeightedEnemyCandidate> candidates = BuildCandidatesOrThrow(pool);
        EnemyData picked = WeightedPick(candidates);
        if (picked == null)
        {
            throw new InvalidOperationException($"地区 {regionCode} 敌人池无有效可抽取条目，流程中断");
        }

        return picked;
    }

    public EnemyData GetEnemyByIdInRegion(string regionCode, string enemyId)
    {
        EventCondition.ValidateRegionCodeOrThrow(regionCode, "EnemyPoolService.regionCode");
        if (string.IsNullOrWhiteSpace(enemyId))
        {
            throw new InvalidOperationException("enemyId 为空，无法开始战斗");
        }

        RegionEnemyPool pool = GetPoolOrThrow(regionCode);
        for (int i = 0; i < pool.entries.Count; i++)
        {
            RegionEnemyEntry entry = pool.entries[i];
            ValidateEntryOrThrow(entry, pool.regionCode, i);

            if (!string.Equals(entry.enemyRef.EnemyId, enemyId, StringComparison.Ordinal))
            {
                continue;
            }

            return entry.enemyRef;
        }

        throw new InvalidOperationException(
            $"当前地区敌人池找不到 enemyId={enemyId}（regionCode={regionCode}）"
        );
    }

    public IReadOnlyList<EnemyData> GetAllEnemiesForValidation()
    {
        EnemyData[] loaded = Resources.LoadAll<EnemyData>(enemyResourcesFolder);
        List<EnemyData> allEnemies = new List<EnemyData>();
        for (int i = 0; i < loaded.Length; i++)
        {
            if (loaded[i] != null)
            {
                allEnemies.Add(loaded[i]);
            }
        }

        return allEnemies;
    }

    public RegionEnemyTable GetRegionEnemyTableForValidation()
    {
        return regionEnemyTable;
    }

    private RegionEnemyPool GetPoolOrThrow(string regionCode)
    {
        if (regionEnemyTable == null)
        {
            throw new InvalidOperationException("RegionEnemyTable 未配置，流程中断");
        }

        if (regionEnemyTable.regionPools == null || regionEnemyTable.regionPools.Count == 0)
        {
            throw new InvalidOperationException("RegionEnemyTable 未配置任何地区池，流程中断");
        }

        for (int i = 0; i < regionEnemyTable.regionPools.Count; i++)
        {
            RegionEnemyPool pool = regionEnemyTable.regionPools[i];
            if (pool == null || string.IsNullOrWhiteSpace(pool.regionCode))
            {
                continue;
            }

            EventCondition.ValidateRegionCodeOrThrow(pool.regionCode, "RegionEnemyTable.regionCode");
            if (string.Equals(pool.regionCode, regionCode, StringComparison.Ordinal))
            {
                if (pool.entries == null || pool.entries.Count == 0)
                {
                    throw new InvalidOperationException($"地区 {regionCode} 未配置敌人条目，流程中断");
                }

                return pool;
            }
        }

        throw new InvalidOperationException($"地区 {regionCode} 未配置敌人池，流程中断");
    }

    private List<WeightedEnemyCandidate> BuildCandidatesOrThrow(RegionEnemyPool pool)
    {
        List<WeightedEnemyCandidate> candidates = new List<WeightedEnemyCandidate>();
        for (int i = 0; i < pool.entries.Count; i++)
        {
            RegionEnemyEntry entry = pool.entries[i];
            ValidateEntryOrThrow(entry, pool.regionCode, i);

            candidates.Add(new WeightedEnemyCandidate
            {
                Enemy = entry.enemyRef,
                Weight = entry.weight
            });
        }

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException($"地区 {pool.regionCode} 敌人池无有效可抽取条目，流程中断");
        }

        return candidates;
    }

    private static void ValidateEntryOrThrow(RegionEnemyEntry entry, string regionCode, int index)
    {
        if (entry == null)
        {
            throw new InvalidOperationException($"敌人池条目为空：regionCode={regionCode}, index={index}");
        }

        if (entry.weight <= 0f)
        {
            throw new InvalidOperationException(
                $"敌人池权重非法：regionCode={regionCode}, index={index}, weight={entry.weight}"
            );
        }

        if (entry.enemyRef == null)
        {
            throw new InvalidOperationException($"敌人池条目 enemyRef 为空：regionCode={regionCode}, index={index}");
        }

        if (string.IsNullOrWhiteSpace(entry.enemyRef.EnemyId))
        {
            throw new InvalidOperationException($"敌人池条目 enemyId 为空：regionCode={regionCode}, index={index}");
        }

        // 地区归属由 RegionEnemyTable 的池 regionCode 决定，EnemyData 不再承载地区字段。
    }

    private EnemyData WeightedPick(List<WeightedEnemyCandidate> candidates)
    {
        float total = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            total += Mathf.Max(0f, candidates[i].Weight);
        }

        if (total <= 0f)
        {
            return null;
        }

        float roll = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            WeightedEnemyCandidate candidate = candidates[i];
            acc += Mathf.Max(0f, candidate.Weight);
            if (roll <= acc)
            {
                return candidate.Enemy;
            }
        }

        return candidates[candidates.Count - 1].Enemy;
    }

    private sealed class WeightedEnemyCandidate
    {
        public EnemyData Enemy;
        public float Weight;
    }
}

