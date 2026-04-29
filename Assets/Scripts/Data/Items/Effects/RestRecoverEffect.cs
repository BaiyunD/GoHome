using UnityEngine;

[CreateAssetMenu(fileName = "RestRecoverEffect", menuName = "GoHome/Item Effects/Rest Recover")]
public sealed class RestRecoverEffect : ItemEffectDefinition
{
    [SerializeField] private int hpRecoverPerLevel;
    [SerializeField] private int energyRecoverPerLevel;
    [SerializeField] private int hungerDeltaPerLevel;

    public void Configure(int hpRecover, int energyRecover, int hungerDelta)
    {
        hpRecoverPerLevel = hpRecover;
        energyRecoverPerLevel = energyRecover;
        hungerDeltaPerLevel = hungerDelta;
    }

    public override void OnRestItemEffect(RestContext context, int level)
    {
        if (context == null || level <= 0)
        {
            return;
        }

        context.DisplayedHpRecover += hpRecoverPerLevel * level;
        context.DisplayedEnergyRecover += energyRecoverPerLevel * level;
        context.DisplayedHungerDelta += hungerDeltaPerLevel * level;
    }

}
