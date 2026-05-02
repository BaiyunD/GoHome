using UnityEngine;

[CreateAssetMenu(fileName = "BlackKnifeRuptureEffect", menuName = "GoHome/Item Effects/Specific/Black Knife Rupture")]
public sealed class BlackKnifeRuptureEffect : ItemEffectDefinition
{
    public override void OnBattleHookItemEffect(BattleEffectContext context, int level)
    {
        if (context == null || level <= 0)
        {
            return;
        }

        if (context.Hook != BattleEffectHook.BeforeAttack
            || context.Owner != BattleEffectOwner.Player
            || context.Attacker == null
            || context.Defender == null)
        {
            return;
        }

        if (BattleManager.Instance == null)
        {
            return;
        }

        int stacks = BattleManager.Instance.AdvancePlayerRuptureStackAndGetCapped();
        float mult = 1f + stacks * (0.04f * level);
        context.FinalDamage = Mathf.Max(0, Mathf.RoundToInt(context.ComputedDamage * mult));
        int displayPercent = 4 * level * stacks;
        context.AppendAttackerPhaseLog($"【割裂】伤害提升[{displayPercent}%]");
    }
}
