using UnityEngine;

[CreateAssetMenu(fileName = "MedkitStackHealEffect", menuName = "GoHome/Item Effects/Specific/Medkit Stack Heal")]
public sealed class MedkitStackHealEffect : ItemEffectDefinition
{
    [SerializeField] private float baseHeal = 30f;
    [SerializeField] private int healBonusPerUse = 1;

    public void Configure(float baseHealValue, int healBonusPerUseValue)
    {
        baseHeal = baseHealValue;
        healBonusPerUse = healBonusPerUseValue;
    }

    public override bool OnUseItemEffect(ItemBase item, int level, out string resultText)
    {
        resultText = string.Empty;
        if (item == null || PlayerResourceService.Instance == null)
        {
            return false;
        }

        float safeBaseHeal = Mathf.Max(0f, baseHeal);
        int safeBonusPerUse = Mathf.Max(0, healBonusPerUse);
        float healAmount = safeBaseHeal + ConsumableRuntimeState.MedkitHealBonus * safeBonusPerUse;
        bool healed = PlayerResourceService.Instance.ApplyDelta(
            PlayerResourceType.HP,
            healAmount,
            "MedkitStackHealEffect.TryApplyUse");
        if (!healed)
        {
            return false;
        }

        ConsumableRuntimeState.IncreaseMedkitHealBonus(safeBonusPerUse);
        resultText = UITexts.FormatUseSuccess(item.DisplayName);
        return true;
    }

}
