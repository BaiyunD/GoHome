using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableUseEffect", menuName = "GoHome/Item Effects/Common/Consumable Use")]
public sealed class CommonConsumableUseEffect : ItemEffectDefinition
{
    [SerializeField] private float hungerDelta;

    public void Configure(float hungerDeltaValue)
    {
        hungerDelta = hungerDeltaValue;
    }

    public override bool OnUseItemEffect(ItemBase item, int level, out string resultText)
    {
        resultText = string.Empty;
        if (item == null || PlayerResourceService.Instance == null)
        {
            return false;
        }

        if (!ApplyHungerOnly())
        {
            return false;
        }

        resultText = UITexts.FormatUseSuccess(item.DisplayName);
        return true;
    }

    private bool ApplyHungerOnly()
    {
        return PlayerResourceService.Instance.ApplyDelta(
            PlayerResourceType.Hunger,
            hungerDelta,
            "ConsumableUseEffect.HungerOnly");
    }
}
