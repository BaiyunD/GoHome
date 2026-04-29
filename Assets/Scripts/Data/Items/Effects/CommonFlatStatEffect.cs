using UnityEngine;

[CreateAssetMenu(fileName = "FlatStatEffect", menuName = "GoHome/Item Effects/Common/Flat Stat")]
public sealed class CommonFlatStatEffect : ItemEffectDefinition
{
    [SerializeField] private float hpMaxPerLevel;
    [SerializeField] private float energyMaxPerLevel;
    [SerializeField] private float attackPerLevel;
    [SerializeField] private float defensePerLevel;

    public void Configure(float hpMaxValue, float energyMaxValue, float attackValue, float defenseValue)
    {
        hpMaxPerLevel = hpMaxValue;
        energyMaxPerLevel = energyMaxValue;
        attackPerLevel = attackValue;
        defensePerLevel = defenseValue;
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
    }

}
