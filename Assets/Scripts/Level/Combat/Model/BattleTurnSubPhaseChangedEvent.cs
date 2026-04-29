public sealed class BattleTurnSubPhaseChangedEvent
{
    public BattleTurnSubPhaseChangedEvent(BattleTurnSubPhase from, BattleTurnSubPhase to, string reason)
    {
        From = from;
        To = to;
        Reason = reason ?? string.Empty;
    }

    public BattleTurnSubPhase From { get; }
    public BattleTurnSubPhase To { get; }
    public string Reason { get; }
}
