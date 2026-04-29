using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance
    {
        get; private set;
    }

    private readonly Dictionary<int, ItemBase> _itemsById = new Dictionary<int, ItemBase>();
    private readonly Dictionary<int, ItemDefinition> _definitionsById = new Dictionary<int, ItemDefinition>();
    private readonly List<GameObject> _runtimeItemObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        Rebuild();
    }

    public void Rebuild()
    {
        _itemsById.Clear();
        _definitionsById.Clear();
        ClearRuntimeItems();
        BuildFromItemDefinitions();
    }

    private void TryRegisterItem(ItemBase item, string sourceName)
    {
        if (item.Id <= 0)
        {
            Debug.LogWarning($"ItemRegistry: 物品 {sourceName} 的 Id<=0，已跳过。");
            return;
        }

        if (_itemsById.ContainsKey(item.Id))
        {
            Debug.LogError($"ItemRegistry: 重复物品Id={item.Id}，来源={sourceName}。请保证唯一。");
            return;
        }

        _itemsById.Add(item.Id, item);
    }

    private void BuildFromItemDefinitions()
    {
        ItemDefinition[] defs = Resources.LoadAll<ItemDefinition>("ItemDefinitions");
        if (defs == null || defs.Length == 0)
        {
            Debug.LogWarning("ItemRegistry: Resources/ItemDefinitions 下未找到任何 ItemDefinition。");
            return;
        }

        for (int i = 0; i < defs.Length; i++)
        {
            ItemDefinition def = defs[i];
            if (def == null) continue;
            if (def.Id <= 0) continue;

            Sprite icon = def.Icon != null ? def.Icon : LoadItemIconById(def.Id);
            string displayName = string.IsNullOrWhiteSpace(def.DisplayName) ? def.name : def.DisplayName;
            string description = string.IsNullOrWhiteSpace(def.Description) ? $"{displayName}。" : def.Description;

            if (!_definitionsById.ContainsKey(def.Id))
            {
                _definitionsById.Add(def.Id, def);
            }

            DefinedItem item = CreateRuntimeItemObject<DefinedItem>(displayName);
            item.InitializeRuntime(def.Id, displayName, def.Kind, description, icon);
            TryRegisterItem(item, $"ItemDefinition:{def.name}");
        }
    }

    private T CreateRuntimeItemObject<T>(string objectName) where T : ItemBase
    {
        GameObject go = new GameObject($"RuntimeItem_{objectName}");
        go.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(go);
        _runtimeItemObjects.Add(go);
        return go.AddComponent<T>();
    }

    private static Sprite LoadItemIconById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return Resources.Load<Sprite>($"Items/Icons/icon_{id}");
    }


    private void ClearRuntimeItems()
    {
        for (int i = 0; i < _runtimeItemObjects.Count; i++)
        {
            if (_runtimeItemObjects[i] != null)
            {
                Destroy(_runtimeItemObjects[i]);
            }
        }
        _runtimeItemObjects.Clear();
    }

    public bool TryGet(int id, out ItemBase item)
    {
        return _itemsById.TryGetValue(id, out item);
    }

    public bool TryGetDefinition(int id, out ItemDefinition definition)
    {
        return _definitionsById.TryGetValue(id, out definition);
    }

    public IReadOnlyDictionary<int, ItemBase> GetAll()
    {
        return _itemsById;
    }

    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;

        foreach (var kv in _itemsById)
        {
            SpecialItem special = kv.Value as SpecialItem;
            if (special == null) continue;
            if (special.Tags == null) continue;

            for (int i = 0; i < special.Tags.Count; i++)
            {
                if (special.Tags[i] == tag)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

