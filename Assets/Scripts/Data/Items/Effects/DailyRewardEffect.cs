using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardEffect", menuName = "GoHome/Item Effects/Specific/Daily Reward")]
public sealed class DailyRewardEffect : ItemEffectDefinition
{
    private const int TRAP_ITEM_ID = 205;
    [SerializeField] private float chancePerDayPercent = 30f;
    [SerializeField] private int[] materialItemIds = new int[0];
    [SerializeField] private int materialCountPerLevel = 1;

    public override void OnRestItemEffect(RestContext context, int level)
    {
        if (context == null || level <= 0)
        {
            return;
        }

        float chance = Mathf.Max(0f, chancePerDayPercent) / 100f;
        if (Random.value > chance)
        {
            return;
        }

        ApplyMaterialRewardOnRest(context, level);
    }

    private void ApplyMaterialRewardOnRest(RestContext context, int level)
    {
        if (InventoryManager.Instance == null || materialItemIds == null || materialItemIds.Length == 0)
        {
            return;
        }

        int idx = Random.Range(0, materialItemIds.Length);
        int itemId = materialItemIds[idx];

        // Trap reward count follows trap level directly.
        int count = Mathf.Max(1, level);
        InventoryManager.Instance.AddItem(itemId, count);

        string lootName = ResolveItemName(itemId);
        string trapName = ResolveItemName(TRAP_ITEM_ID);
        context.AddItemTriggeredLog(trapName, $"获得[{lootName}]*{count}");
    }

    private static string ResolveItemName(int itemId)
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
}
