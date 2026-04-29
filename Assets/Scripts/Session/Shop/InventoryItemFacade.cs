using UnityEngine;

public struct InventoryItemSnapshot
{
    public int ItemId;
    public string ItemName;
    public string ItemDescription;
    public Sprite ItemIcon;
    public int OwnedCount;
}

public sealed class InventoryItemFacade
{
    public bool TryGetSnapshot(int itemId, out InventoryItemSnapshot snapshot)
    {
        snapshot = default;
        if (itemId <= 0)
        {
            return false;
        }

        string itemName = $"Item({itemId})";
        string itemDescription = "暂无描述";
        Sprite itemIcon = null;
        if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGet(itemId, out ItemBase item) && item != null)
        {
            itemName = item.DisplayName;
            itemDescription = string.IsNullOrWhiteSpace(item.Description) ? "暂无描述" : item.Description;
            itemIcon = item.Icon;
        }

        if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGetDefinition(itemId, out ItemDefinition definition) && definition != null)
        {
            if (string.IsNullOrWhiteSpace(itemDescription))
            {
                itemDescription = "暂无描述";
            }
            if (string.IsNullOrWhiteSpace(itemDescription) || itemDescription == "暂无描述")
            {
                itemDescription = string.IsNullOrWhiteSpace(definition.Description) ? "暂无描述" : definition.Description;
            }
            if (itemIcon == null)
            {
                itemIcon = definition.Icon;
            }
        }

        int ownedCount = 0;
        if (InventoryManager.Instance != null)
        {
            ownedCount = InventoryManager.Instance.GetItemCount(itemId);
        }

        snapshot = new InventoryItemSnapshot
        {
            ItemId = itemId,
            ItemName = itemName,
            ItemDescription = itemDescription,
            ItemIcon = itemIcon,
            OwnedCount = ownedCount
        };
        return true;
    }
}
