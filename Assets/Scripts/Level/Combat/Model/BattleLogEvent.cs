public enum BattleLogEventType
{
    ActionHint = 0,
    SettlementHint = 1,
    Attack = 2
}

public sealed class BattleLogEvent
{
    public BattleLogEvent(BattleLogEventType eventType, string message)
    {
        EventType = eventType;
        Message = message ?? string.Empty;
    }

    public BattleLogEventType EventType { get; }
    public string Message { get; }
}
