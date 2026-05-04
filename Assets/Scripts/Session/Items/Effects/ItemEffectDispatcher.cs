using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class AdvanceSupplyEffectDispatchResult
{
    public int ItemId;
    public string ItemDisplayName;
    public ItemEffectSource Source;
    public int EffectDisplayPriority;
    public string EffectId;
    public RegionLootService.LootRollResult LootResult;
}

public static class ItemEffectDispatcher
{
    private static readonly HashSet<int> WarnedMissingEffectItemIds = new HashSet<int>();

    public static bool OnPassiveItemEffect(
        int itemId,
        int level,
        PassiveAccumulator accumulator,
        out ItemEffectSource source)
    {
        source = ItemEffectSource.None;
        if (!TryGetOrderedGroups(itemId, out List<ItemEffectDefinition> commonEffects, out List<ItemEffectDefinition> specificEffects, out source))
        {
            return false;
        }

        for (int i = 0; i < commonEffects.Count; i++)
        {
            ItemEffectDefinition effectDefinition = commonEffects[i];
            if (effectDefinition == null)
            {
                continue;
            }

            effectDefinition.OnPassiveItemEffect(accumulator, level);
        }

        for (int i = 0; i < specificEffects.Count; i++)
        {
            ItemEffectDefinition effectDefinition = specificEffects[i];
            if (effectDefinition == null)
            {
                continue;
            }

            effectDefinition.OnPassiveItemEffect(accumulator, level);
        }

        return true;
    }

    public static bool OnAdvanceSupplyItemEffect(
        int itemId,
        AdvanceSupplyEffectContext context,
        out RegionLootService.LootRollResult result,
        out ItemEffectSource source)
    {
        return OnAdvanceSupplyItemEffect(
            itemId,
            context,
            out result,
            out source,
            out _,
            out _);
    }

    private static bool OnAdvanceSupplyItemEffect(
        int itemId,
        AdvanceSupplyEffectContext context,
        out RegionLootService.LootRollResult result,
        out ItemEffectSource source,
        out int effectDisplayPriority,
        out string effectId)
    {
        result = null;
        source = ItemEffectSource.None;
        effectDisplayPriority = int.MaxValue;
        effectId = string.Empty;
        if (!TryGetOrderedGroups(itemId, out List<ItemEffectDefinition> commonEffects, out List<ItemEffectDefinition> specificEffects, out source))
        {
            return false;
        }

        if (context != null)
        {
            context.TriggerItemId = itemId;
            context.TriggerItemLevel = InventoryManager.Instance != null
                ? Mathf.Max(0, InventoryManager.Instance.GetItemCount(itemId))
                : 0;
        }

        for (int i = 0; i < commonEffects.Count; i++)
        {
            ItemEffectDefinition effectDefinition = commonEffects[i];
            if (effectDefinition == null)
            {
                continue;
            }

            if (effectDefinition.OnAdvanceSupplyItemEffect(context, out result))
            {
                effectDisplayPriority = effectDefinition.DisplayPriority;
                effectId = effectDefinition.EffectId ?? string.Empty;
                return true;
            }
        }

        for (int i = 0; i < specificEffects.Count; i++)
        {
            ItemEffectDefinition effectDefinition = specificEffects[i];
            if (effectDefinition == null)
            {
                continue;
            }

            if (effectDefinition.OnAdvanceSupplyItemEffect(context, out result))
            {
                effectDisplayPriority = effectDefinition.DisplayPriority;
                effectId = effectDefinition.EffectId ?? string.Empty;
                return true;
            }
        }

        return false;
    }

    public static List<AdvanceSupplyEffectDispatchResult> ApplyAdvanceSupplyForInventory(
        AdvanceSupplyEffectContext context)
    {
        List<AdvanceSupplyEffectDispatchResult> results = new List<AdvanceSupplyEffectDispatchResult>();
        if (context == null || InventoryManager.Instance == null)
        {
            return results;
        }

        List<int> itemIds = new List<int>();
        foreach (KeyValuePair<int, int> pair in InventoryManager.inventoryDict)
        {
            if (pair.Value <= 0)
            {
                continue;
            }

            itemIds.Add(pair.Key);
        }

        for (int i = 0; i < itemIds.Count; i++)
        {
            int itemId = itemIds[i];
            bool triggered = OnAdvanceSupplyItemEffect(
                itemId,
                context,
                out RegionLootService.LootRollResult lootResult,
                out ItemEffectSource source,
                out int effectDisplayPriority,
                out string effectId);
            if (!triggered || lootResult == null)
            {
                continue;
            }

            results.Add(new AdvanceSupplyEffectDispatchResult
            {
                ItemId = itemId,
                ItemDisplayName = ResolveItemDisplayName(itemId),
                Source = source,
                EffectDisplayPriority = effectDisplayPriority,
                EffectId = effectId,
                LootResult = lootResult
            });
        }

        results.Sort(CompareAdvanceSupplyResultOrder);
        return results;
    }

    public static bool OnRestItemEffect(int itemId, int level, RestContext context, out ItemEffectSource source)
    {
        source = ItemEffectSource.None;
        if (!TryGetOrderedGroups(itemId, out List<ItemEffectDefinition> commonEffects, out List<ItemEffectDefinition> specificEffects, out source))
        {
            return false;
        }

        string displayName = ResolveItemDisplayName(itemId);
        try
        {
            if (context != null)
            {
                context.CurrentRestItemId = itemId;
                context.CurrentRestItemDisplayName = displayName;
            }

            for (int i = 0; i < commonEffects.Count; i++)
            {
                ItemEffectDefinition effectDefinition = commonEffects[i];
                if (effectDefinition == null)
                {
                    continue;
                }

                effectDefinition.OnRestItemEffect(context, level);
            }

            for (int i = 0; i < specificEffects.Count; i++)
            {
                ItemEffectDefinition effectDefinition = specificEffects[i];
                if (effectDefinition == null)
                {
                    continue;
                }

                effectDefinition.OnRestItemEffect(context, level);
            }
        }
        finally
        {
            if (context != null)
            {
                context.CurrentRestItemId = 0;
                context.CurrentRestItemDisplayName = null;
            }
        }

        return true;
    }

    public static bool OnUseItemEffect(ItemBase item, out string resultText, out ItemEffectSource source)
    {
        resultText = string.Empty;
        source = ItemEffectSource.None;
        if (item == null || InventoryManager.Instance == null)
        {
            return false;
        }

        int level = Mathf.Max(0, InventoryManager.Instance.GetItemCount(item.Id));
        if (!TryGetOrderedGroups(item.Id, out List<ItemEffectDefinition> commonEffects, out List<ItemEffectDefinition> specificEffects, out source))
        {
            return false;
        }

        bool anyApplied = false;
        string firstResultText = string.Empty;
        for (int i = 0; i < commonEffects.Count; i++)
        {
            ItemEffectDefinition effectDefinition = commonEffects[i];
            if (effectDefinition == null)
            {
                continue;
            }

            if (effectDefinition.OnUseItemEffect(item, level, out string currentResultText))
            {
                anyApplied = true;
                if (string.IsNullOrWhiteSpace(firstResultText) && !string.IsNullOrWhiteSpace(currentResultText))
                {
                    firstResultText = currentResultText;
                }
            }
        }

        for (int i = 0; i < specificEffects.Count; i++)
        {
            ItemEffectDefinition effectDefinition = specificEffects[i];
            if (effectDefinition == null)
            {
                continue;
            }

            if (effectDefinition.OnUseItemEffect(item, level, out string currentResultText))
            {
                anyApplied = true;
                if (string.IsNullOrWhiteSpace(firstResultText) && !string.IsNullOrWhiteSpace(currentResultText))
                {
                    firstResultText = currentResultText;
                }
            }
        }

        resultText = firstResultText;
        return anyApplied;
    }

    public static void OnBattleHookItemEffect(BattleEffectContext context)
    {
        if (context == null || InventoryManager.Instance == null)
        {
            return;
        }

        IReadOnlyDictionary<int, int> inventory = InventoryManager.inventoryDict;
        foreach (KeyValuePair<int, int> entry in inventory)
        {
            int itemId = entry.Key;
            int level = Mathf.Max(0, entry.Value);
            if (level <= 0)
            {
                continue;
            }

            if (!TryGetOrderedGroups(itemId, out List<ItemEffectDefinition> commonEffects, out List<ItemEffectDefinition> specificEffects, out ItemEffectSource _))
            {
                continue;
            }

            for (int i = 0; i < commonEffects.Count; i++)
            {
                ItemEffectDefinition effectDefinition = commonEffects[i];
                if (effectDefinition == null)
                {
                    continue;
                }

                effectDefinition.OnBattleHookItemEffect(context, level);
            }

            for (int i = 0; i < specificEffects.Count; i++)
            {
                ItemEffectDefinition effectDefinition = specificEffects[i];
                if (effectDefinition == null)
                {
                    continue;
                }

                effectDefinition.OnBattleHookItemEffect(context, level);
            }
        }
    }

    /// <summary>战斗胜利主结算之后：按物品 Id 升序遍历背包，拼接各效果返回的追加叙述。</summary>
    public static string AppendBattleVictorySettlement(BattleVictorySettlementContext ctx)
    {
        if (ctx == null || InventoryManager.Instance == null)
        {
            return string.Empty;
        }

        List<int> itemIds = new List<int>();
        foreach (KeyValuePair<int, int> pair in InventoryManager.inventoryDict)
        {
            if (pair.Value > 0)
            {
                itemIds.Add(pair.Key);
            }
        }

        itemIds.Sort();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < itemIds.Count; i++)
        {
            int itemId = itemIds[i];
            int level = Mathf.Max(0, InventoryManager.Instance.GetItemCount(itemId));
            if (level <= 0)
            {
                continue;
            }

            if (!TryGetOrderedGroups(itemId, out List<ItemEffectDefinition> commonEffects, out List<ItemEffectDefinition> specificEffects, out _))
            {
                continue;
            }

            for (int c = 0; c < commonEffects.Count; c++)
            {
                ItemEffectDefinition effectDefinition = commonEffects[c];
                if (effectDefinition == null)
                {
                    continue;
                }

                string fragment = effectDefinition.OnBattleVictorySettlement(ctx, level);
                if (!string.IsNullOrEmpty(fragment))
                {
                    sb.Append(fragment);
                }
            }

            for (int s = 0; s < specificEffects.Count; s++)
            {
                ItemEffectDefinition effectDefinition = specificEffects[s];
                if (effectDefinition == null)
                {
                    continue;
                }

                string fragment = effectDefinition.OnBattleVictorySettlement(ctx, level);
                if (!string.IsNullOrEmpty(fragment))
                {
                    sb.Append(fragment);
                }
            }
        }

        return sb.ToString();
    }

    private static string ResolveItemDisplayName(int itemId)
    {
        if (ItemRegistry.Instance != null
            && ItemRegistry.Instance.TryGet(itemId, out ItemBase item)
            && item != null
            && !string.IsNullOrWhiteSpace(item.DisplayName))
        {
            return item.DisplayName;
        }

        return $"Item({itemId})";
    }

    private static int CompareAdvanceSupplyResultOrder(
        AdvanceSupplyEffectDispatchResult left,
        AdvanceSupplyEffectDispatchResult right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int leftPriority = left.EffectDisplayPriority;
        int rightPriority = right.EffectDisplayPriority;
        if (leftPriority != rightPriority)
        {
            return leftPriority.CompareTo(rightPriority);
        }

        string leftEffectId = left.EffectId ?? string.Empty;
        string rightEffectId = right.EffectId ?? string.Empty;
        int effectIdCompare = string.Compare(leftEffectId, rightEffectId, System.StringComparison.Ordinal);
        if (effectIdCompare != 0)
        {
            return effectIdCompare;
        }

        return left.ItemId.CompareTo(right.ItemId);
    }

    private static bool TryGetOrderedGroups(
        int itemId,
        out List<ItemEffectDefinition> commonEffects,
        out List<ItemEffectDefinition> specificEffects,
        out ItemEffectSource source)
    {
        commonEffects = new List<ItemEffectDefinition>();
        specificEffects = new List<ItemEffectDefinition>();
        source = ItemEffectSource.None;
        if (!ItemEffectFactory.TryCreateGroupedByItemId(itemId, out ItemEffectDefinitionGroups groups, out source)
            || groups == null
            || !groups.HasAnyEffects)
        {
            if (ItemRegistry.Instance != null
                && ItemRegistry.Instance.TryGetDefinition(itemId, out ItemDefinition definition)
                && definition != null
                && definition.Kind != ItemKind.Material)
            {
                WarnMissingEffectOnce(itemId, definition.DisplayName);
            }
            return false;
        }

        commonEffects.AddRange(groups.CommonEffects);
        specificEffects.AddRange(groups.SpecificEffects);
        commonEffects.Sort(CompareEffectDefinitionOrder);
        specificEffects.Sort(CompareEffectDefinitionOrder);
        return true;
    }

    private static void WarnMissingEffectOnce(int itemId, string displayName)
    {
        if (!WarnedMissingEffectItemIds.Add(itemId))
        {
            return;
        }

        Debug.LogWarning(
            $"ItemEffectDispatcher.TryGetOrderedGroups -> item={itemId}({displayName}) 未命中可用效果资产，已禁用该物品效果。");
    }

    private static int CompareEffectDefinitionOrder(ItemEffectDefinition left, ItemEffectDefinition right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        string leftId = left.EffectId ?? string.Empty;
        string rightId = right.EffectId ?? string.Empty;
        bool leftEmpty = string.IsNullOrWhiteSpace(leftId);
        bool rightEmpty = string.IsNullOrWhiteSpace(rightId);
        if (leftEmpty && rightEmpty)
        {
            return 0;
        }

        if (leftEmpty)
        {
            return 1;
        }

        if (rightEmpty)
        {
            return -1;
        }

        return string.Compare(leftId, rightId, System.StringComparison.Ordinal);
    }
}
