public sealed class BattleEndEvent
{
    public BattleEndEvent(BattleResult result, string narration)
    {
        Result = result;
        Narration = narration ?? string.Empty;
    }

    public BattleResult Result { get; }
    public string Narration { get; }
}

