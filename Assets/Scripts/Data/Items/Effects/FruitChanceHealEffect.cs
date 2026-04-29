using UnityEngine;

[CreateAssetMenu(fileName = "FruitChanceHealEffect", menuName = "GoHome/Item Effects/Specific/Fruit Chance Heal")]
public sealed class FruitChanceHealEffect : ItemEffectDefinition
{
    [SerializeField] private float chanceToHeal = 0.15f;
    [SerializeField] private float healAmount = 1f;

    public void Configure(float chanceToHealValue, float healAmountValue)
    {
        chanceToHeal = chanceToHealValue;
        healAmount = healAmountValue;
    }

    public override bool OnUseItemEffect(ItemBase item, int level, out string resultText)
    {
        resultText = string.Empty;
        if (item == null || PlayerResourceService.Instance == null)
        {
            return false;
        }

        float chance = Mathf.Clamp01(chanceToHeal);
        if (chance > 0f && Random.value < chance)
        {
            PlayerResourceService.Instance.ApplyDelta(
                PlayerResourceType.Health,
                healAmount,
                "FruitChanceHealEffect.TryApplyUse");
        }

        resultText = UITexts.FormatUseSuccess(item.DisplayName);
        return true;
    }

}
