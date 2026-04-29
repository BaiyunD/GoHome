using UnityEngine;

[CreateAssetMenu(fileName = "StatPassiveEffect", menuName = "GoHome/Item Effects/Stat Passive")]
public sealed class StatPassiveEffect : ItemEffectDefinition
{
    [SerializeField] private float hpMaxPerLevel;
    [SerializeField] private float energyMaxPerLevel;
    [SerializeField] private float hungerMaxPerLevel;
    [SerializeField] private float attackPerLevel;
    [SerializeField] private float defensePerLevel;
    [SerializeField] private float criticalRatePerLevel;
    [SerializeField] private float blockRatePerLevel;
    [SerializeField] private float dodgeRatePerLevel;

    public void Configure(
        float hpMax,
        float energyMax,
        float hungerMax,
        float attack,
        float defense,
        float criticalRate,
        float blockRate,
        float dodgeRate)
    {
        hpMaxPerLevel = hpMax;
        energyMaxPerLevel = energyMax;
        hungerMaxPerLevel = hungerMax;
        attackPerLevel = attack;
        defensePerLevel = defense;
        criticalRatePerLevel = criticalRate;
        blockRatePerLevel = blockRate;
        dodgeRatePerLevel = dodgeRate;
    }

    public override void OnPassiveItemEffect(PassiveAccumulator accumulator, int level)
    {
        if (accumulator == null || level <= 0)
        {
            return;
        }

        accumulator.HpMaxBonus += hpMaxPerLevel * level;
        accumulator.EnergyMaxBonus += energyMaxPerLevel * level;
        accumulator.HungerMaxBonus += hungerMaxPerLevel * level;
        accumulator.AttackBonus += attackPerLevel * level;
        accumulator.DefenseBonus += defensePerLevel * level;
        accumulator.CriticalRateBonus += criticalRatePerLevel * level;
        accumulator.BlockRateBonus += blockRatePerLevel * level;
        accumulator.DodgeRateBonus += dodgeRatePerLevel * level;
    }

}
