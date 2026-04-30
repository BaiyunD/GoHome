using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class RegionLootService
{
    public sealed class LootGrant
    {
        public LootRewardType RewardType;
        public int ItemId;
        public int Count;
        public float MoneyAmount;
    }

    public sealed class LootRollResult
    {
        public readonly List<LootGrant> Grants = new List<LootGrant>();
        public bool IsEmpty => Grants.Count == 0;
    }

    public static LootRollResult RollAndGrant(string regionCode, RegionLootTable table)
    {
        if (table == null)
        {
            throw new InvalidOperationException("RegionLootTable 未配置，流程中断");
        }

        EventCondition.ValidateRegionCodeOrThrow(regionCode, "RegionLootTable.regionCode");
        RegionLootPool pool = GetPoolOrThrow(regionCode, table);
        int targetKinds = RollLootKinds(table);
        LootRollResult result = new LootRollResult();

        if (targetKinds <= 0)
        {
            return result;
        }

        List<LootEntry> pickedEntries = PickUniqueEntries(pool.entries, targetKinds);
        for (int i = 0; i < pickedEntries.Count; i++)
        {
            LootEntry entry = pickedEntries[i];
            int count = GetCount(entry);
            if (count <= 0)
            {
                continue;
            }

            if (entry.rewardType == LootRewardType.Item)
            {
                if (entry.itemId <= 0)
                {
                    throw new InvalidOperationException("物资池存在非法 itemId（<=0），流程中断");
                }

                if (InventoryManager.Instance == null)
                {
                    throw new InvalidOperationException("InventoryManager 未挂载，流程中断");
                }

                InventoryManager.Instance.AddItem(entry.itemId, count);
                result.Grants.Add(new LootGrant
                {
                    RewardType = LootRewardType.Item,
                    ItemId = entry.itemId,
                    Count = count
                });
                continue;
            }

            if (PlayerResourceService.Instance == null)
            {
                throw new InvalidOperationException("玩家金钱属性不可用，流程中断");
            }

            float amount = MoneyUtil.CentsToYuan(
                MoneyUtil.YuanToCents(Mathf.Max(0f, entry.moneyAmount) * count));
            if (MoneyUtil.YuanToCents(amount) <= 0)
            {
                continue;
            }

            bool granted = PlayerResourceService.Instance.ApplyDelta(
                PlayerResourceType.Money,
                amount,
                "RegionLootService.Grant"
            );
            if (!granted)
            {
                throw new InvalidOperationException("玩家金钱属性不可用，流程中断");
            }
            result.Grants.Add(new LootGrant
            {
                RewardType = LootRewardType.Money,
                MoneyAmount = amount
            });
        }

        return result;
    }

    public static string BuildNarration(LootRollResult result)
    {
        if (result == null || result.IsEmpty)
        {
            return "你这次什么都没发现。";
        }

        StringBuilder sb = new StringBuilder("发现");
        for (int i = 0; i < result.Grants.Count; i++)
        {
            LootGrant grant = result.Grants[i];
            sb.Append("【");
            if (grant.RewardType == LootRewardType.Money)
            {
                sb.Append("金钱】*").Append(grant.MoneyAmount.ToString("0.00"));
                continue;
            }

            sb.Append(GetItemNameSafe(grant.ItemId)).Append("】*").Append(grant.Count);
        }

        return sb.ToString();
    }

    private static RegionLootPool GetPoolOrThrow(string regionCode, RegionLootTable table)
    {
        if (table.regionPools == null || table.regionPools.Count == 0)
        {
            throw new InvalidOperationException("RegionLootTable 未配置任何地区池，流程中断");
        }

        for (int i = 0; i < table.regionPools.Count; i++)
        {
            RegionLootPool pool = table.regionPools[i];
            if (pool == null || string.IsNullOrWhiteSpace(pool.regionCode))
            {
                continue;
            }

            EventCondition.ValidateRegionCodeOrThrow(pool.regionCode, "RegionLootTable.regionCode");
            if (string.Equals(pool.regionCode, regionCode, StringComparison.Ordinal))
            {
                if (pool.entries == null || pool.entries.Count == 0)
                {
                    throw new InvalidOperationException($"地区 {regionCode} 未配置物资条目，流程中断");
                }
                return pool;
            }
        }

        throw new InvalidOperationException($"地区 {regionCode} 未配置物资池，流程中断");
    }

    private static int RollLootKinds(RegionLootTable table)
    {
        float p0 = Mathf.Max(0f, table.noLootProbability);
        float p1 = Mathf.Max(0f, table.oneLootProbability);
        float p2 = Mathf.Max(0f, table.twoLootProbability);
        float p3 = Mathf.Max(0f, table.threeLootProbability);
        float total = p0 + p1 + p2 + p3;
        if (total <= 0f)
        {
            return 1;
        }

        float roll = UnityEngine.Random.Range(0f, total);
        if (roll < p0) return 0;
        roll -= p0;
        if (roll < p1) return 1;
        roll -= p1;
        if (roll < p2) return 2;
        return 3;
    }

    private static List<LootEntry> PickUniqueEntries(List<LootEntry> entries, int count)
    {
        List<LootEntry> candidates = new List<LootEntry>();
        if (entries != null)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                LootEntry entry = entries[i];
                if (entry == null || entry.weight <= 0f)
                {
                    continue;
                }

                if (entry.rewardType == LootRewardType.Item && entry.itemId <= 0)
                {
                    continue;
                }

                if (entry.rewardType == LootRewardType.Money && MoneyUtil.YuanToCents(entry.moneyAmount) <= 0)
                {
                    continue;
                }

                candidates.Add(entry);
            }
        }

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("物资池无有效可抽取条目，流程中断");
        }

        List<LootEntry> picked = new List<LootEntry>();
        int targetCount = Mathf.Min(Mathf.Max(0, count), candidates.Count);
        for (int i = 0; i < targetCount; i++)
        {
            LootEntry selected = WeightedPick(candidates);
            if (selected == null)
            {
                break;
            }

            picked.Add(selected);
            candidates.Remove(selected);
        }

        return picked;
    }

    private static LootEntry WeightedPick(List<LootEntry> entries)
    {
        float total = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            total += Mathf.Max(0f, entries[i].weight);
        }

        if (total <= 0f)
        {
            return null;
        }

        float roll = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            LootEntry entry = entries[i];
            acc += Mathf.Max(0f, entry.weight);
            if (roll <= acc)
            {
                return entry;
            }
        }

        return entries[entries.Count - 1];
    }

    private static int GetCount(LootEntry entry)
    {
        return entry.countOption == LootCountOption.Two ? 2 : 1;
    }

    private static string GetItemNameSafe(int id)
    {
        if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGet(id, out ItemBase item) && item != null)
        {
            return item.DisplayName;
        }

        return $"Item({id})";
    }
}
