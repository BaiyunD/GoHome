using System;
using System.Collections.Generic;

public static class RegionBootstrapValidator
{
    public static void ValidateOrThrow(
        IReadOnlyList<MainRegionData> mainRegions,
        EventManager eventManager,
        EnemyPoolService enemyPoolService,
        RegionLootTable regionLootTable
    )
    {
        if (mainRegions == null || mainRegions.Count == 0)
        {
            throw new InvalidOperationException("[REGION_EMPTY] 启动失败：未加载到任何主地区配置。");
        }

        List<RegionNodeContract> contracts = BuildContractsOrThrow(mainRegions);
        HashSet<string> validRegionCodes = BuildRegionCodeSet(contracts);
        HashSet<string> validEnemyIds = ValidateEnemiesOrThrow(enemyPoolService, validRegionCodes);
        ValidateEnemyPoolsOrThrow(enemyPoolService, validRegionCodes, validEnemyIds);
        ValidateLootPoolsOrThrow(regionLootTable, validRegionCodes);
        ValidateEventsOrThrow(eventManager, validRegionCodes, validEnemyIds);
    }

    private static List<RegionNodeContract> BuildContractsOrThrow(IReadOnlyList<MainRegionData> mainRegions)
    {
        List<RegionNodeContract> contracts = new List<RegionNodeContract>();
        HashSet<string> mainIds = new HashSet<string>(StringComparer.Ordinal);
        HashSet<int> mainIndexes = new HashSet<int>();
        HashSet<string> nodeIds = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string> regionCodes = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < mainRegions.Count; i++)
        {
            MainRegionData main = mainRegions[i];
            if (main == null)
            {
                throw new InvalidOperationException($"[REGION_NULL] 主地区配置为空，索引={i}。");
            }

            if (!mainIndexes.Add(main.MainRegionId))
            {
                throw new InvalidOperationException(
                    $"[REGION_DUP_MAIN_INDEX] 主地区整型索引重复：{main.MainRegionId}。"
                );
            }

            if (!mainIds.Add(main.MainRegionKey))
            {
                throw new InvalidOperationException(
                    $"[REGION_DUP_MAIN_ID] 主地区字符串ID重复：{main.MainRegionKey}。"
                );
            }

            IReadOnlyList<SubRegionInfo> subs = main.SubRegions;
            if (subs == null || subs.Count == 0)
            {
                throw new InvalidOperationException(
                    $"[REGION_MISSING_SUB_LINK] 主地区缺少分地区关联：mainId={main.MainRegionKey}。"
                );
            }

            HashSet<int> subIndexes = new HashSet<int>();
            HashSet<string> subIds = new HashSet<string>(StringComparer.Ordinal);
            for (int j = 0; j < subs.Count; j++)
            {
                SubRegionInfo sub = subs[j];
                if (sub == null)
                {
                    throw new InvalidOperationException(
                        $"[REGION_NULL_SUB] 分地区配置为空：mainId={main.MainRegionKey}, subIdx={j}。"
                    );
                }

                if (!subIndexes.Add(sub.SubRegionId))
                {
                    throw new InvalidOperationException(
                        $"[REGION_DUP_SUB_INDEX] 分地区整型索引重复：mainId={main.MainRegionKey}, subIndex={sub.SubRegionId}。"
                    );
                }

                if (!subIds.Add(sub.SubRegionKey))
                {
                    throw new InvalidOperationException(
                        $"[REGION_DUP_SUB_ID] 分地区字符串ID重复：mainId={main.MainRegionKey}, subId={sub.SubRegionKey}。"
                    );
                }

                RegionNodeContract contract = new RegionNodeContract(
                    main.MainRegionKey,
                    main.MainRegionId,
                    sub.SubRegionKey,
                    sub.SubRegionId
                );

                string nodeId = $"{contract.MainRegionId}/{contract.SubRegionId}";
                if (!nodeIds.Add(nodeId))
                {
                    throw new InvalidOperationException($"[REGION_DUP_NODE_ID] 地区节点ID重复：{nodeId}。");
                }

                if (!regionCodes.Add(contract.RegionCode))
                {
                    throw new InvalidOperationException(
                        $"[REGION_DUP_REGION_CODE] 地区编码重复：{contract.RegionCode}。"
                    );
                }

                contracts.Add(contract);
            }
        }

        return contracts;
    }

    private static HashSet<string> BuildRegionCodeSet(IReadOnlyList<RegionNodeContract> contracts)
    {
        HashSet<string> validRegionCodes = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < contracts.Count; i++)
        {
            validRegionCodes.Add(contracts[i].RegionCode);
        }

        return validRegionCodes;
    }

    private static HashSet<string> ValidateEnemiesOrThrow(
        EnemyPoolService enemyPoolService,
        HashSet<string> validRegionCodes
    )
    {
        if (enemyPoolService == null)
        {
            throw new InvalidOperationException("[REF_ENEMY_POOL_MISSING] 启动失败：EnemyPoolService 未挂载。");
        }

        IReadOnlyList<EnemyData> enemies = enemyPoolService.GetAllEnemiesForValidation();
        HashSet<string> enemyIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyData enemy = enemies[i];
            if (enemy == null)
            {
                throw new InvalidOperationException($"[REF_ENEMY_NULL] 敌人配置为空，索引={i}。");
            }

            if (string.IsNullOrWhiteSpace(enemy.EnemyId))
            {
                throw new InvalidOperationException($"[REF_ENEMY_ID_EMPTY] 敌人ID为空：asset={enemy.name}。");
            }

            if (!enemyIds.Add(enemy.EnemyId))
            {
                throw new InvalidOperationException($"[REF_ENEMY_ID_DUP] 敌人ID重复：enemyId={enemy.EnemyId}。");
            }
        }

        return enemyIds;
    }

    private static void ValidateEnemyPoolsOrThrow(
        EnemyPoolService enemyPoolService,
        HashSet<string> validRegionCodes,
        HashSet<string> validEnemyIds
    )
    {
        RegionEnemyTable table = enemyPoolService.GetRegionEnemyTableForValidation();
        if (table == null)
        {
            throw new InvalidOperationException("[ENEMY_POOL_TABLE_MISSING] 启动失败：RegionEnemyTable 未配置。");
        }

        if (table.regionPools == null || table.regionPools.Count == 0)
        {
            throw new InvalidOperationException("[ENEMY_POOL_EMPTY] 启动失败：RegionEnemyTable 无任何地区敌人池。");
        }

        HashSet<string> poolCodes = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < table.regionPools.Count; i++)
        {
            RegionEnemyPool pool = table.regionPools[i];
            ValidateEnemyPoolHeaderOrThrow(pool, i, validRegionCodes, poolCodes);

            bool hasPositiveWeight = false;
            for (int j = 0; j < pool.entries.Count; j++)
            {
                ValidateEnemyEntryOrThrow(pool.entries[j], pool.regionCode, j, validEnemyIds);
                hasPositiveWeight = true;
            }

            if (!hasPositiveWeight)
            {
                throw new InvalidOperationException(
                    $"[ENEMY_POOL_WEIGHT_INVALID] 敌人池无可用权重：regionCode={pool.regionCode}。"
                );
            }
        }

        foreach (string validCode in validRegionCodes)
        {
            if (!poolCodes.Contains(validCode))
            {
                throw new InvalidOperationException(
                    $"[ENEMY_POOL_REGION_MISSING] 分地区敌人池缺失：regionCode={validCode}。"
                );
            }
        }
    }

    private static void ValidateEnemyPoolHeaderOrThrow(
        RegionEnemyPool pool,
        int poolIndex,
        HashSet<string> validRegionCodes,
        HashSet<string> poolCodes
    )
    {
        if (pool == null)
        {
            throw new InvalidOperationException($"[ENEMY_POOL_NULL] 敌人池配置为空，索引={poolIndex}。");
        }

        EventCondition.ValidateRegionCodeOrThrow(pool.regionCode, "RegionEnemyTable.regionCode");
        if (!validRegionCodes.Contains(pool.regionCode))
        {
            throw new InvalidOperationException(
                $"[REF_REGION_NOT_FOUND] 敌人池引用不存在地区：regionCode={pool.regionCode}。"
            );
        }

        if (!poolCodes.Add(pool.regionCode))
        {
            throw new InvalidOperationException($"[ENEMY_POOL_DUP_REGION] 同地区敌人池重复：regionCode={pool.regionCode}。");
        }

        if (pool.entries == null || pool.entries.Count == 0)
        {
            throw new InvalidOperationException($"[ENEMY_POOL_EMPTY_ENTRIES] 敌人池为空：regionCode={pool.regionCode}。");
        }
    }

    private static void ValidateEnemyEntryOrThrow(
        RegionEnemyEntry entry,
        string poolRegionCode,
        int entryIndex,
        HashSet<string> validEnemyIds
    )
    {
        if (entry == null)
        {
            throw new InvalidOperationException(
                $"[ENEMY_POOL_ENTRY_NULL] 敌人池条目为空：regionCode={poolRegionCode}, index={entryIndex}。"
            );
        }

        if (entry.weight <= 0f)
        {
            throw new InvalidOperationException(
                $"[ENEMY_POOL_WEIGHT_INVALID] 敌人池权重非法：regionCode={poolRegionCode}, index={entryIndex}, weight={entry.weight}。"
            );
        }

        if (entry.enemyRef == null)
        {
            throw new InvalidOperationException(
                $"[ENEMY_POOL_ENTRY_INVALID] 敌人池条目 enemyRef 为空：regionCode={poolRegionCode}, index={entryIndex}。"
            );
        }

        if (string.IsNullOrWhiteSpace(entry.enemyRef.EnemyId))
        {
            throw new InvalidOperationException(
                $"[ENEMY_POOL_ENTRY_INVALID] 敌人池条目 enemyId 为空：regionCode={poolRegionCode}, index={entryIndex}。"
            );
        }

        if (!validEnemyIds.Contains(entry.enemyRef.EnemyId))
        {
            throw new InvalidOperationException(
                $"[ENEMY_POOL_REF_NOT_FOUND] 敌人池引用不存在 enemyId={entry.enemyRef.EnemyId}：regionCode={poolRegionCode}。"
            );
        }
    }

    private static void ValidateLootPoolsOrThrow(RegionLootTable table, HashSet<string> validRegionCodes)
    {
        if (table == null)
        {
            throw new InvalidOperationException("[POOL_TABLE_MISSING] 启动失败：RegionLootTable 未配置。");
        }

        if (table.regionPools == null || table.regionPools.Count == 0)
        {
            throw new InvalidOperationException("[POOL_EMPTY] 启动失败：RegionLootTable 无任何地区反馈池。");
        }

        HashSet<string> poolCodes = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < table.regionPools.Count; i++)
        {
            RegionLootPool pool = table.regionPools[i];
            if (pool == null)
            {
                throw new InvalidOperationException($"[POOL_NULL] 反馈池配置为空，索引={i}。");
            }

            EventCondition.ValidateRegionCodeOrThrow(pool.regionCode, "RegionLootTable.regionCode");
            if (!validRegionCodes.Contains(pool.regionCode))
            {
                throw new InvalidOperationException(
                    $"[REF_REGION_NOT_FOUND] 反馈池引用不存在地区：regionCode={pool.regionCode}。"
                );
            }

            if (!poolCodes.Add(pool.regionCode))
            {
                throw new InvalidOperationException($"[POOL_DUP_REGION] 同地区反馈池重复：regionCode={pool.regionCode}。");
            }

            if (pool.entries == null || pool.entries.Count == 0)
            {
                throw new InvalidOperationException($"[POOL_EMPTY_ENTRIES] 反馈池为空：regionCode={pool.regionCode}。");
            }

            bool hasPositiveWeight = false;
            for (int j = 0; j < pool.entries.Count; j++)
            {
                LootEntry entry = pool.entries[j];
                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"[POOL_ENTRY_NULL] 反馈池条目为空：regionCode={pool.regionCode}, index={j}。"
                    );
                }

                if (entry.weight <= 0f)
                {
                    throw new InvalidOperationException(
                        $"[POOL_WEIGHT_INVALID] 反馈池权重非法：regionCode={pool.regionCode}, index={j}, weight={entry.weight}。"
                    );
                }

                hasPositiveWeight = true;

                if (entry.rewardType == LootRewardType.Item && entry.itemId <= 0)
                {
                    throw new InvalidOperationException(
                        $"[POOL_ENTRY_INVALID] Item条目缺少合法itemId：regionCode={pool.regionCode}, index={j}。"
                    );
                }

                if (entry.rewardType == LootRewardType.Money && MoneyUtil.YuanToCents(entry.moneyAmount) <= 0)
                {
                    throw new InvalidOperationException(
                        $"[POOL_ENTRY_INVALID] Money条目缺少合法moneyAmount：regionCode={pool.regionCode}, index={j}。"
                    );
                }
            }

            if (!hasPositiveWeight)
            {
                throw new InvalidOperationException(
                    $"[POOL_WEIGHT_INVALID] 反馈池无可用权重：regionCode={pool.regionCode}。"
                );
            }
        }

        foreach (string validCode in validRegionCodes)
        {
            if (!poolCodes.Contains(validCode))
            {
                throw new InvalidOperationException(
                    $"[POOL_REGION_MISSING] 分地区反馈池缺失：regionCode={validCode}。"
                );
            }
        }
    }

    private static void ValidateEventsOrThrow(
        EventManager eventManager,
        HashSet<string> validRegionCodes,
        HashSet<string> validEnemyIds
    )
    {
        if (eventManager == null)
        {
            throw new InvalidOperationException("[REF_EVENT_MANAGER_MISSING] 启动失败：EventManager 未挂载。");
        }

        RegionEventTable table = eventManager.GetRegionEventTableForValidation();
        if (table == null)
        {
            throw new InvalidOperationException("[EVENT_POOL_TABLE_MISSING] 启动失败：RegionEventTable 未配置。");
        }

        if (table.regionPools == null || table.regionPools.Count == 0)
        {
            throw new InvalidOperationException("[EVENT_POOL_EMPTY] 启动失败：RegionEventTable 无任何地区事件池。");
        }

        HashSet<string> poolCodes = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < table.regionPools.Count; i++)
        {
            RegionEventPool pool = table.regionPools[i];
            if (pool == null)
            {
                throw new InvalidOperationException($"[EVENT_POOL_NULL] 事件池配置为空，索引={i}。");
            }

            EventCondition.ValidateRegionCodeOrThrow(pool.regionCode, "RegionEventTable.regionCode");
            if (!validRegionCodes.Contains(pool.regionCode))
            {
                throw new InvalidOperationException(
                    $"[REF_REGION_NOT_FOUND] 事件池引用不存在地区：regionCode={pool.regionCode}。"
                );
            }

            if (!poolCodes.Add(pool.regionCode))
            {
                throw new InvalidOperationException($"[EVENT_POOL_DUP_REGION] 同地区事件池重复：regionCode={pool.regionCode}。");
            }

            if (pool.entries == null || pool.entries.Count == 0)
            {
                throw new InvalidOperationException($"[EVENT_POOL_EMPTY_ENTRIES] 事件池为空：regionCode={pool.regionCode}。");
            }

            bool hasPositiveWeight = false;
            for (int j = 0; j < pool.entries.Count; j++)
            {
                RegionEventEntry entry = pool.entries[j];
                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"[EVENT_POOL_ENTRY_NULL] 事件池条目为空：regionCode={pool.regionCode}, index={j}。"
                    );
                }

                if (entry.weight <= 0f)
                {
                    throw new InvalidOperationException(
                        $"[EVENT_POOL_WEIGHT_INVALID] 事件池权重非法：regionCode={pool.regionCode}, index={j}, weight={entry.weight}。"
                    );
                }

                if (entry.eventRef == null)
                {
                    throw new InvalidOperationException(
                        $"[EVENT_POOL_ENTRY_INVALID] 事件池条目 eventRef 为空：regionCode={pool.regionCode}, index={j}。"
                    );
                }

                ValidateEventByConfig(entry.eventRef, validRegionCodes, validEnemyIds);

                hasPositiveWeight = true;
            }

            if (!hasPositiveWeight)
            {
                throw new InvalidOperationException(
                    $"[EVENT_POOL_WEIGHT_INVALID] 事件池无可用权重：regionCode={pool.regionCode}。"
                );
            }
        }

        foreach (string validCode in validRegionCodes)
        {
            if (!poolCodes.Contains(validCode))
            {
                throw new InvalidOperationException(
                    $"[EVENT_POOL_REGION_MISSING] 分地区事件池缺失：regionCode={validCode}。"
                );
            }
        }
    }

    private static void ValidateEventByConfig(
        GameEvent gameEvent,
        HashSet<string> validRegionCodes,
        HashSet<string> validEnemyIds
    )
    {
        ValidateEventEnemyReference(gameEvent.defaultResults, validEnemyIds, gameEvent.name, "defaultResults");
        if (gameEvent.options == null) return;

        for (int j = 0; j < gameEvent.options.Count; j++)
        {
            EventOption option = gameEvent.options[j];
            if (option == null || option.eventOutcomes == null) continue;
            for (int k = 0; k < option.eventOutcomes.Count; k++)
            {
                EventOutcome outcome = option.eventOutcomes[k];
                if (outcome == null) continue;
                ValidateEventEnemyReference(
                    outcome.results,
                    validEnemyIds,
                    gameEvent.name,
                    $"option[{j}].outcome[{k}]"
                );
            }
        }
    }

    private static void ValidateEventEnemyReference(
        IReadOnlyList<EventResult> results,
        HashSet<string> validEnemyIds,
        string eventName,
        string source
    )
    {
        if (results == null) return;

        for (int i = 0; i < results.Count; i++)
        {
            EventResult result = results[i];
            if (result == null || result.resultType != EventResultType.EnemyEncounter)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(result.enemyId))
            {
                throw new InvalidOperationException(
                    $"[REF_EVENT_ENEMY_EMPTY] 事件敌人引用为空：event={eventName}, source={source}, index={i}。"
                );
            }

            if (!validEnemyIds.Contains(result.enemyId))
            {
                throw new InvalidOperationException(
                    $"[REF_EVENT_ENEMY_NOT_FOUND] 事件敌人引用不存在：event={eventName}, enemyId={result.enemyId}。"
                );
            }
        }
    }
}
