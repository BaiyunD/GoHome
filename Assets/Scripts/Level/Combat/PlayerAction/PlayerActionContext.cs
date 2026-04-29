public sealed class PlayerActionContext
{
    public PlayerActionContext(CharacterRuntimeStats player, CharacterRuntimeStats enemy, string enemyName)
    {
        Player = player;
        Enemy = enemy;
        EnemyName = enemyName ?? string.Empty;
    }

    public CharacterRuntimeStats Player { get; }
    public CharacterRuntimeStats Enemy { get; }
    public string EnemyName { get; }
}
