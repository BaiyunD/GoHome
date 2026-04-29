using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ItemEffectAutoDiscovery
{
    private const string RESOURCES_ROOT = "ItemEffects";

    private static readonly Regex LeadingItemIdRegex = new Regex(@"^(?<id>\d+)_", RegexOptions.Compiled);
    private static readonly Regex AnyItemIdRegex = new Regex(@"(?<!\d)(?<id>\d{2,})(?!\d)", RegexOptions.Compiled);
    private static readonly Dictionary<int, ItemEffectDefinitionGroups> CacheByItemId =
        new Dictionary<int, ItemEffectDefinitionGroups>();
    private static bool _initialized;

    public static bool TryGetGroups(int itemId, out ItemEffectDefinitionGroups groups)
    {
        EnsureInitialized();
        return CacheByItemId.TryGetValue(itemId, out groups) && groups != null && groups.HasAnyEffects;
    }

    public static void Reload()
    {
        CacheByItemId.Clear();
        _initialized = false;
        EnsureInitialized();
    }

    private static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        CacheByItemId.Clear();
        ItemEffectDefinition[] allEffects = Resources.LoadAll<ItemEffectDefinition>(RESOURCES_ROOT);
        for (int i = 0; i < allEffects.Length; i++)
        {
            ItemEffectDefinition effect = allEffects[i];
            if (effect == null)
            {
                continue;
            }

            if (!TryResolveItemId(effect, out int itemId))
            {
                Debug.LogWarning(
                    $"ItemEffectAutoDiscovery -> 无法从资产名/路径解析 itemId，已跳过：{effect.name}");
                continue;
            }

            if (!CacheByItemId.TryGetValue(itemId, out ItemEffectDefinitionGroups groups) || groups == null)
            {
                groups = new ItemEffectDefinitionGroups();
                CacheByItemId[itemId] = groups;
            }

            bool isSpecific = IsSpecificEffect(effect);
            List<ItemEffectDefinition> target = isSpecific ? groups.SpecificEffects : groups.CommonEffects;
            if (!target.Contains(effect))
            {
                target.Add(effect);
            }
        }

        foreach (KeyValuePair<int, ItemEffectDefinitionGroups> pair in CacheByItemId)
        {
            ItemEffectDefinitionGroups groups = pair.Value;
            if (groups == null)
            {
                continue;
            }

            groups.CommonEffects.Sort(CompareEffectOrder);
            groups.SpecificEffects.Sort(CompareEffectOrder);
        }
    }

    private static bool TryResolveItemId(ItemEffectDefinition effect, out int itemId)
    {
        itemId = 0;
        string source = effect != null ? effect.name : string.Empty;
        if (TryParseItemIdFromText(source, out itemId))
        {
            return true;
        }

        string effectId = effect != null ? effect.EffectId : string.Empty;
        if (TryParseItemIdFromText(effectId, out itemId))
        {
            return true;
        }

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(effect);
        if (TryParseItemIdFromPath(path, out itemId))
        {
            return true;
        }
#endif

        return false;
    }

    private static bool TryParseItemIdFromText(string text, out int itemId)
    {
        itemId = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        Match match = LeadingItemIdRegex.Match(text.Trim());
        if (match.Success && int.TryParse(match.Groups["id"].Value, out itemId))
        {
            return true;
        }

        match = AnyItemIdRegex.Match(text);
        return match.Success && int.TryParse(match.Groups["id"].Value, out itemId);
    }

    private static bool TryParseItemIdFromPath(string assetPath, out int itemId)
    {
        itemId = 0;
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return false;
        }

        string normalized = assetPath.Replace('\\', '/');
        string[] parts = normalized.Split('/');
        for (int i = 0; i < parts.Length; i++)
        {
            if (TryParseItemIdFromText(parts[i], out itemId))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSpecificEffect(ItemEffectDefinition effect)
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(effect);
        if (!string.IsNullOrWhiteSpace(path))
        {
            string normalized = path.Replace('\\', '/');
            if (normalized.Contains("/Specific/"))
            {
                return true;
            }

            if (normalized.Contains("/Common/"))
            {
                return false;
            }
        }
#endif
        string name = effect != null ? effect.name : string.Empty;
        return name.IndexOf("specific", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static int CompareEffectOrder(ItemEffectDefinition left, ItemEffectDefinition right)
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
            return string.Compare(left.name, right.name, System.StringComparison.Ordinal);
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
