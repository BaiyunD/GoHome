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
}
