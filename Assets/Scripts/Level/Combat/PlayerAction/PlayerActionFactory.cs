public static class PlayerActionFactory
{
    public static IPlayerAction Create(PlayerBattleCommandType commandType)
    {
        switch (commandType)
        {
            case PlayerBattleCommandType.NormalAttack:
                return new NormalAttackAction();
            case PlayerBattleCommandType.Flee:
                return new FleeAction();
            default:
                return null;
        }
    }
}
