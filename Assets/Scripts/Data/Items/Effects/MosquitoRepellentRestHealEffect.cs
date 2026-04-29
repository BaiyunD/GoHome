using UnityEngine;

[CreateAssetMenu(
    fileName = "MosquitoRepellentRestHealEffect",
    menuName = "GoHome/Item Effects/Specific/Mosquito Repellent Rest Heal")]
public sealed class MosquitoRepellentRestHealEffect : ItemEffectDefinition
{
    [SerializeField] private int hpRecoverPerLevel = 30;

    public void Configure(int hpRecoverPerLevelValue)
    {
        hpRecoverPerLevel = hpRecoverPerLevelValue;
    }

    public override void OnRestItemEffect(RestContext context, int level)
    {
        if (context == null || level <= 0)
        {
            return;
        }

        int safeRecover = Mathf.Max(0, hpRecoverPerLevel);
        context.DisplayedHpRecover += safeRecover * level;
    }

}
