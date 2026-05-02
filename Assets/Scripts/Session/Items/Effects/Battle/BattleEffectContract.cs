using System.Collections.Generic;

public enum BattleEffectOwner
{
    Player = 0,
    Enemy = 1
}

public enum BattleEffectHook
{
    BattleStart = 0,
    BeforeAttack = 1,
    BeforeReceiveHit = 2,
    AfterAttack = 3,
    AfterReceiveHit = 4,
    TryFlee = 5,
    BattleEnd = 6
}

public sealed class BattleEffectContext
{
    public BattleEffectHook Hook;
    public BattleEffectOwner Owner;
    public CharacterRuntimeStats Attacker;
    public CharacterRuntimeStats Defender;
    public int ComputedDamage;
    public int FinalDamage;
    public float FleeRate;
    public int TurnIndex;
    public BattleResult EndResult;

    private readonly List<string> _attackerPhaseLogParts = new List<string>(2);
    private readonly List<string> _defenderPhaseLogParts = new List<string>(2);
    private readonly List<string> _afterAttackPhaseLogParts = new List<string>(2);

    public void AppendAttackerPhaseLog(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
        {
            return;
        }

        _attackerPhaseLogParts.Add(fragment);
    }

    public void AppendDefenderPhaseLog(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
        {
            return;
        }

        _defenderPhaseLogParts.Add(fragment);
    }

    public string BuildAttackerPhaseLogSuffix()
    {
        return _attackerPhaseLogParts.Count == 0 ? string.Empty : string.Concat(_attackerPhaseLogParts);
    }

    public string BuildDefenderPhaseLogSuffix()
    {
        return _defenderPhaseLogParts.Count == 0 ? string.Empty : string.Concat(_defenderPhaseLogParts);
    }

    public void AppendAfterAttackPhaseLog(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
        {
            return;
        }

        _afterAttackPhaseLogParts.Add(fragment);
    }

    public string BuildAfterAttackPhaseLogSuffix()
    {
        return _afterAttackPhaseLogParts.Count == 0 ? string.Empty : string.Concat(_afterAttackPhaseLogParts);
    }
}
