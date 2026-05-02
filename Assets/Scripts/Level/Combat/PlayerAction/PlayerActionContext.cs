public sealed class PlayerActionContext
{
    public PlayerActionContext(
        CharacterRuntimeStats player,
        CharacterRuntimeStats enemy,
        string enemyName,
        float damageBoostMultiplier = 1f,
        float damageReductionMultiplier = 0f
    )
    {
        Player = player;
        Enemy = enemy;
        EnemyName = enemyName ?? string.Empty;
        DamageBoostMultiplier = damageBoostMultiplier;
        DamageReductionMultiplier = damageReductionMultiplier;
    }

    public CharacterRuntimeStats Player { get; }
    public CharacterRuntimeStats Enemy { get; }
    public string EnemyName { get; }
    public float DamageBoostMultiplier { get; }
    public float DamageReductionMultiplier { get; }
}
