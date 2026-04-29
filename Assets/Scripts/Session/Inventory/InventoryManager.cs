using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public static Dictionary<int, int> inventoryDict = new Dictionary<int, int>();

    public event Action OnItemChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 默认物资初始化已迁移到 InitializeNewGameInventory，避免 Continue 读档被 Start 污染。
    }

    /// <summary>
    /// 往仓库里根据id添加对应物品数量，若无则添加该物体id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public void AddItem(int id, int count)
    {
        if (HasItem(id))
        {
            inventoryDict[id] += count;
        }
        else
        {
            inventoryDict.Add(id, count);
        }
        Debug.LogFormat("添加了{0}个{1}", count, GetItemNameSafe(id));
        OnItemChanged?.Invoke();
    }

    public void RemoveItem(int id, int count)
    {
        if (HasItem(id))
        {
            if (inventoryDict[id] >= count)
            {
                inventoryDict[id] -= count;
                Debug.LogFormat("删除了{0}个{1}", count, GetItemNameSafe(id));
                OnItemChanged?.Invoke();
            }
            else
            {
                Debug.Log("物品数量不足");
            }
        }
        else
        {
            Debug.Log("仓库没有该物品");
        }
    }

    // 旧 Item(ScriptableObject) 体系已弃用：统一走 id->quantity

    /// <summary>
    /// 编号版本s
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool HasItem(int id)
    {
        if (inventoryDict.ContainsKey(id))
        {
            return true;
        }
        return false;
    }

    public bool HasSpecialItem(int id)
    {
        if (ItemRegistry.Instance == null)
        {
            return false;
        }

        if (!ItemRegistry.Instance.TryGet(id, out ItemBase item) || item == null)
        {
            return false;
        }

        return item is SpecialItem && GetItemCount(id) > 0;
    }

    public int GetItemCount(int id)
    {
        if (HasItem(id))
        {
            return inventoryDict[id];
        }
        else
        {
            return 0;
        }
    }

    public List<SaveInventoryEntryData> ExportEntries()
    {
        List<SaveInventoryEntryData> entries = new List<SaveInventoryEntryData>();
        foreach (KeyValuePair<int, int> pair in inventoryDict)
        {
            entries.Add(
                new SaveInventoryEntryData
                {
                    ItemId = pair.Key,
                    Count = pair.Value
                }
            );
        }

        return entries;
    }

    public void ReplaceAllItems(IEnumerable<SaveInventoryEntryData> entries)
    {
        ReplaceAllItems(entries, true);
    }

    public void ReplaceAllItems(IEnumerable<SaveInventoryEntryData> entries, bool notifyChange)
    {
        inventoryDict.Clear();
        if (entries != null)
        {
            foreach (SaveInventoryEntryData entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.Count <= 0)
                {
                    continue;
                }

                inventoryDict[entry.ItemId] = entry.Count;
            }
        }

        if (notifyChange)
        {
            OnItemChanged?.Invoke();
        }
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
