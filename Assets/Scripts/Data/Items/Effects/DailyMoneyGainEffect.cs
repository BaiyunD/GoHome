using System.Globalization;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyMoneyGainEffect", menuName = "GoHome/Item Effects/Specific/Daily Money Gain")]
public sealed class DailyMoneyGainEffect : ItemEffectDefinition
{
    private const int ITEM_ID = 208;
    [SerializeField] private float moneyPerLevel = 0.1f;

    public void Configure(float moneyPerLevelValue)
    {
        moneyPerLevel = moneyPerLevelValue;
    }

    public override void OnRestItemEffect(RestContext context, int level)
    {
        if (context == null || level <= 0 || PlayerResourceService.Instance == null)
        {
            return;
        }

        float amount = Mathf.Max(0f, moneyPerLevel) * level;
        if (amount <= 0f)
        {
            return;
        }

        bool granted = PlayerResourceService.Instance.ApplyDelta(
            PlayerResourceType.Money,
            amount,
            "DailyMoneyGainEffect.OnRestItemEffect");
        if (!granted)
        {
            return;
        }

        string amountText = amount.ToString("0.##", CultureInfo.InvariantCulture);
        context.AddItemTriggeredLog(ResolveItemName(ITEM_ID), $"获得{amountText}元");
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
