public enum GameLaunchMode
{
    None,
    NewGame,
    Continue
}

public static class GameLaunchContext
{
    private static SaveData _pendingSaveData;
    private static string _pendingStartPresetId;

    public static GameLaunchMode PendingMode { get; private set; } = GameLaunchMode.None;

    public static void PrepareNewGame(string startPresetId)
    {
        PendingMode = GameLaunchMode.NewGame;
        _pendingSaveData = null;
        _pendingStartPresetId = startPresetId;
    }

    public static void PrepareContinue(SaveData saveData)
    {
        PendingMode = GameLaunchMode.Continue;
        _pendingSaveData = saveData;
        _pendingStartPresetId = null;
    }

    public static string ConsumePendingStartPresetId()
    {
        string startPresetId = _pendingStartPresetId;
        _pendingStartPresetId = null;
        return startPresetId;
    }

    public static SaveData ConsumePendingSaveData()
    {
        SaveData saveData = _pendingSaveData;
        _pendingSaveData = null;
        PendingMode = GameLaunchMode.None;
        return saveData;
    }
}
