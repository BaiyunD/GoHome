using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    [Header("基础信息")]
    [SerializeField] private int id;
    [SerializeField] private string displayName;
    [SerializeField] private ItemKind kind;
    [TextArea(2, 6)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public int Id => id;
    public string DisplayName => displayName;
    public ItemKind Kind => kind;
    public string Description => description;
    public Sprite Icon => icon;

    public virtual string GetDescription(int level)
    {
        return description;
    }

    public void InitializeRuntime(
        int itemId,
        string itemDisplayName,
        ItemKind itemKind,
        string itemDescription,
        Sprite itemIcon)
    {
        id = itemId;
        displayName = itemDisplayName;
        kind = itemKind;
        description = itemDescription;
        icon = itemIcon;
    }
}

