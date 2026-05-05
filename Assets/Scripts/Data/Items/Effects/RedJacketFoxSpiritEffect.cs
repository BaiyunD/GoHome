using UnityEngine;

[CreateAssetMenu(fileName = "RedJacketFoxSpiritEffect", menuName = "GoHome/Item Effects/Specific/Red Jacket Fox Spirit")]
public sealed class RedJacketFoxSpiritEffect : ItemEffectDefinition
{
    [SerializeField] private float triggerChancePerLevel = 0.10f;
    [SerializeField] private float defenseFractionPerStack = 0.20f;
    [SerializeField] private float healMaxHpFraction = 0.05f;

    public override void OnBattleHookItemEffect(BattleEffectContext context, int level)
    {
        if (context == null
            || level <= 0
            || context.Hook != BattleEffectHook.AfterReceiveHit
            || context.Owner != BattleEffectOwner.Player
            || context.Attacker == null
            || context.Defender == null)
        {
            return;
        }

        float chance = Mathf.Clamp01(Mathf.Max(0f, triggerChancePerLevel) * level);
        if (chance <= 0f || Random.value >= chance)
        {
            return;
        }

        int healAttempt = Mathf.Max(0, Mathf.RoundToInt(context.Defender.MaxHp * Mathf.Max(0f, healMaxHpFraction)));
        context.Defender.Heal(healAttempt);

        int deltaDef = 0;
        if (BattleManager.Instance != null && BattleManager.Instance.TryIncrementFoxSpiritDefenseStack())
        {
            int baseDef = BattleManager.Instance.PlayerBattleStartDefenseSnapshot;
            deltaDef = Mathf.Max(0, Mathf.RoundToInt(baseDef * Mathf.Max(0f, defenseFractionPerStack)));
            if (deltaDef > 0)
            {
                context.Defender.AddDefenseModifier(deltaDef);
            }
        }

        context.AppendDefenderAfterReceivePhaseLog(
            $"【火狐之灵】提升{deltaDef}点防御，恢复{healAttempt}点生命值");
    }
}
