public sealed class BattleAttackEvent
{
    public BattleAttackEvent(string attackerName, string defenderName, string skillLabel, int damage)
    {
        AttackerName = attackerName ?? string.Empty;
        DefenderName = defenderName ?? string.Empty;
        SkillLabel = skillLabel ?? string.Empty;
        Damage = damage;
    }

    public string AttackerName { get; }
    public string DefenderName { get; }
    public string SkillLabel { get; }
    public int Damage { get; }
}
