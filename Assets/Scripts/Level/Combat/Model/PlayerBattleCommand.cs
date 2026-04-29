public enum PlayerBattleCommandType
{
    NormalAttack = 0,
    Flee = 1
}

public sealed class PlayerBattleCommand
{
    public PlayerBattleCommand(PlayerBattleCommandType commandType)
    {
        CommandType = commandType;
    }

    public PlayerBattleCommandType CommandType { get; }
}
