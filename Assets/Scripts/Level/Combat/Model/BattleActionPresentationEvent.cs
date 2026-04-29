public enum BattleActionActor
{
    Player = 0,
    Enemy = 1
}

public sealed class BattleActionPresentationEvent
{
    public BattleActionPresentationEvent(BattleActionActor actor, BattlePhase phase)
    {
        Actor = actor;
        Phase = phase;
    }

    public BattleActionActor Actor { get; }
    public BattlePhase Phase { get; }
}
