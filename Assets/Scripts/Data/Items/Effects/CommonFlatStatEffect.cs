using UnityEngine;

[CreateAssetMenu(fileName = "FlatStatEffect", menuName = "GoHome/Item Effects/Common/Flat Stat")]
public sealed class CommonFlatStatEffect : ItemEffectDefinition
{
    [SerializeField] private float hpMaxPerLevel;
    [SerializeField] private float energyMaxPerLevel;
    [SerializeField] private float attackPerLevel;
    [SerializeField] private float defensePerLevel;

    [Tooltip("0~100，每级增加的暴击率百分点")]
    [SerializeField] private float criticalRatePerLevel;
    [Tooltip("每级增加的暴击伤害倍率百分比（如 150 表示 150% 伤害）")]
    [SerializeField] private float criticalDamagePerLevel;
    [Tooltip("0~100，每级增加的格挡率百分点")]
    [SerializeField] private float blockRatePerLevel;
    [Tooltip("0~100，每级增加的闪避率百分点")]
    [SerializeField] private float dodgeRatePerLevel;
    [Tooltip("0~100，每级增加的逃跑率百分点")]
    [SerializeField] private float escapeRatePerLevel;

    public void Configure(
        float hpMaxValue,
        float energyMaxValue,
        float attackValue,
        float defenseValue,
        float criticalRateValue = 0f,
        float criticalDamageValue = 0f,
        float blockRateValue = 0f,
        float dodgeRateValue = 0f,
        float escapeRateValue = 0f)
    {
        hpMaxPerLevel = hpMaxValue;
        energyMaxPerLevel = energyMaxValue;
        attackPerLevel = attackValue;
        defensePerLevel = defenseValue;
        criticalRatePerLevel = criticalRateValue;
        criticalDamagePerLevel = criticalDamageValue;
        blockRatePerLevel = blockRateValue;
        dodgeRatePerLevel = dodgeRateValue;
        escapeRatePerLevel = escapeRateValue;
    }

    public override void OnPassiveItemEffect(PassiveAccumulator accumulator, int level)
    {
        if (accumulator == null || level <= 0)
        {
            return;
        }

        accumulator.HpMaxBonus += hpMaxPerLevel * level;
        accumulator.EnergyMaxBonus += energyMaxPerLevel * level;
        accumulator.AttackBonus += attackPerLevel * level;
        accumulator.DefenseBonus += defensePerLevel * level;
        accumulator.CriticalRateBonus += criticalRatePerLevel * level;
        accumulator.CriticalDamageBonus += criticalDamagePerLevel * level;
        accumulator.BlockRateBonus += blockRatePerLevel * level;
        accumulator.DodgeRateBonus += dodgeRatePerLevel * level;
        accumulator.EscapeRateBonus += escapeRatePerLevel * level;
    }

}
