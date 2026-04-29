using System.Collections.Generic;
using UnityEngine;

public enum ItemEffectSource
{
    None = 0,
    Asset = 1
}

public sealed class ItemEffectDefinitionGroups
{
    public readonly List<ItemEffectDefinition> CommonEffects = new List<ItemEffectDefinition>();
    public readonly List<ItemEffectDefinition> SpecificEffects = new List<ItemEffectDefinition>();

    public bool HasAnyEffects => CommonEffects.Count > 0 || SpecificEffects.Count > 0;
}

public static class ItemEffectFactory
{
    public static bool TryCreateByItemId(int itemId, out ItemEffectDefinition effectDefinition, out bool fromAsset)
    {
        bool success = TryCreateByItemId(itemId, out effectDefinition, out ItemEffectSource source);
        fromAsset = source == ItemEffectSource.Asset;
        return success;
    }

    public static bool TryCreateByItemId(
        int itemId,
        out ItemEffectDefinition effectDefinition,
        out ItemEffectSource source)
    {
        effectDefinition = null;
        source = ItemEffectSource.None;
        if (itemId <= 0)
        {
            return false;
        }

        if (TryCreateFromDefinitionAsset(itemId, out effectDefinition))
        {
            source = ItemEffectSource.Asset;
            return true;
        }

        return false;
    }

    public static bool TryCreateAllByItemId(
        int itemId,
        out List<ItemEffectDefinition> effectDefinitions,
        out ItemEffectSource source)
    {
        effectDefinitions = new List<ItemEffectDefinition>();
        source = ItemEffectSource.None;
        if (itemId <= 0)
        {
            return false;
        }

        if (TryCreateGroupedByItemId(itemId, out ItemEffectDefinitionGroups groups, out source))
        {
            effectDefinitions.AddRange(groups.CommonEffects);
            effectDefinitions.AddRange(groups.SpecificEffects);
            return true;
        }

        return false;
    }

    public static bool TryCreateGroupedByItemId(
        int itemId,
        out ItemEffectDefinitionGroups effectGroups,
        out ItemEffectSource source)
    {
        effectGroups = new ItemEffectDefinitionGroups();
        source = ItemEffectSource.None;
        if (itemId <= 0)
        {
            return false;
        }

        if (TryCreateFromDefinitionAssets(itemId, out effectGroups))
        {
            source = ItemEffectSource.Asset;
            return true;
        }

        return false;
    }

    private static bool TryCreateFromDefinitionAsset(int itemId, out ItemEffectDefinition effectDefinition)
    {
        effectDefinition = null;
        if (!TryCreateFromDefinitionAssets(itemId, out ItemEffectDefinitionGroups effectGroups))
        {
            return false;
        }

        if (effectGroups.CommonEffects.Count > 0)
        {
            effectDefinition = effectGroups.CommonEffects[0];
            return true;
        }

        if (effectGroups.SpecificEffects.Count > 0)
        {
            effectDefinition = effectGroups.SpecificEffects[0];
            return true;
        }

        return false;
    }

    private static bool TryCreateFromDefinitionAssets(int itemId, out ItemEffectDefinitionGroups effectGroups)
    {
        effectGroups = null;
        ItemEffectDefinitionGroups discoveredGroups = null;
        bool hasDiscovered = ItemEffectAutoDiscovery.TryGetGroups(itemId, out discoveredGroups)
            && discoveredGroups != null
            && discoveredGroups.HasAnyEffects;

        if (ItemRegistry.Instance == null)
        {
            if (hasDiscovered)
            {
                effectGroups = new ItemEffectDefinitionGroups();
                AddUniqueEffects(discoveredGroups.CommonEffects, effectGroups.CommonEffects);
                AddUniqueEffects(discoveredGroups.SpecificEffects, effectGroups.SpecificEffects);
                return effectGroups.HasAnyEffects;
            }

            return false;
        }

        if (!ItemRegistry.Instance.TryGetDefinition(itemId, out ItemDefinition definition) || definition == null)
        {
            if (hasDiscovered)
            {
                effectGroups = new ItemEffectDefinitionGroups();
                AddUniqueEffects(discoveredGroups.CommonEffects, effectGroups.CommonEffects);
                AddUniqueEffects(discoveredGroups.SpecificEffects, effectGroups.SpecificEffects);
                return effectGroups.HasAnyEffects;
            }

            return false;
        }

        effectGroups = new ItemEffectDefinitionGroups();
        if (hasDiscovered)
        {
            AddUniqueEffects(discoveredGroups.CommonEffects, effectGroups.CommonEffects);
            AddUniqueEffects(discoveredGroups.SpecificEffects, effectGroups.SpecificEffects);
        }

        if (!definition.TryGetGroupedEffectDefinitions(
                out List<ItemEffectDefinition> commonEffects,
                out List<ItemEffectDefinition> specificEffects))
        {
            return effectGroups.HasAnyEffects;
        }

        AddUniqueEffects(commonEffects, effectGroups.CommonEffects);
        AddUniqueEffects(specificEffects, effectGroups.SpecificEffects);
        return effectGroups.HasAnyEffects;
    }

    private static void AddUniqueEffects(
        List<ItemEffectDefinition> source,
        List<ItemEffectDefinition> target)
    {
        if (source == null || target == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            ItemEffectDefinition effect = source[i];
            if (effect == null || target.Contains(effect))
            {
                continue;
            }

            target.Add(effect);
        }
    }

}
