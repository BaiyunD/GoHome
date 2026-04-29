using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "GoHome/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("标识")]
    [Tooltip("新体系ID（按分段自动分配：材料001+，消耗品101+，道具201+，特殊301+）")]
    [SerializeField] private int id;

    [Header("基础信息")]
    [SerializeField] private ItemKind kind;
    [SerializeField] private string displayName;
    [TextArea(2, 6)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("通用效果资产列表（可选）")]
    [SerializeField] private List<ItemEffectDefinition> commonEffectDefinitions = new List<ItemEffectDefinition>();
    [Header("特殊效果资产列表（可选）")]
    [SerializeField] private List<ItemEffectDefinition> specificEffectDefinitions = new List<ItemEffectDefinition>();

    public int Id => id;
    public ItemKind Kind => kind;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public IReadOnlyList<ItemEffectDefinition> CommonEffectDefinitions => commonEffectDefinitions;
    public IReadOnlyList<ItemEffectDefinition> SpecificEffectDefinitions => specificEffectDefinitions;

    public bool TryGetGroupedEffectDefinitions(
        out List<ItemEffectDefinition> commonEffects,
        out List<ItemEffectDefinition> specificEffects)
    {
        commonEffects = new List<ItemEffectDefinition>();
        specificEffects = new List<ItemEffectDefinition>();

        AddValidEffects(commonEffectDefinitions, commonEffects);
        AddValidEffects(specificEffectDefinitions, specificEffects);
        if (commonEffects.Count > 0 || specificEffects.Count > 0)
        {
            return true;
        }

        return false;
    }

    private static void AddValidEffects(List<ItemEffectDefinition> source, List<ItemEffectDefinition> target)
    {
        if (source == null || target == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            ItemEffectDefinition effect = source[i];
            if (effect != null)
            {
                target.Add(effect);
            }
        }
    }

#if UNITY_EDITOR
    public void EditorSetId(int value)
    {
        id = value;
    }
#endif
}

