using System.Globalization;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyMoneyGainEffect", menuName = "GoHome/Item Effects/Specific/Daily Money Gain")]
public sealed class DailyMoneyGainEffect : ItemEffectDefinition
{
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
        context.AddItemTriggeredLog(ResolveRestItemLogName(context), $"获得{amountText}元");
    }

    private static string ResolveRestItemLogName(RestContext context)
    {
        if (context == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(context.CurrentRestItemDisplayName))
        {
            return context.CurrentRestItemDisplayName;
        }

        if (context.CurrentRestItemId > 0)
        {
            return $"Item({context.CurrentRestItemId})";
        }

        return string.Empty;
    }

}
