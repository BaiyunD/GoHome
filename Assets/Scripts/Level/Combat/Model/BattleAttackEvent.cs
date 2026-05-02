public sealed class BattleAttackEvent
{
    public BattleAttackEvent(
        string attackerName,
        string defenderName,
        string skillLabel,
        int damage,
        bool isCritical = false,
        bool isBlocked = false,
        bool isDodged = false
    )
    {
        AttackerName = attackerName ?? string.Empty;
        DefenderName = defenderName ?? string.Empty;
        SkillLabel = skillLabel ?? string.Empty;
        Damage = damage;
        IsCritical = isCritical;
        IsBlocked = isBlocked;
        IsDodged = isDodged;
    }

    public string AttackerName { get; }
    public string DefenderName { get; }
    public string SkillLabel { get; }
    public int Damage { get; }
    public bool IsCritical { get; }
    public bool IsBlocked { get; }
    public bool IsDodged { get; }
}
