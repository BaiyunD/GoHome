using UnityEngine;

public readonly struct NormalAttackResolution
{
    public NormalAttackResolution(int damage, bool isCritical, bool isBlocked, bool isDodged)
    {
        Damage = damage;
        IsCritical = isCritical;
        IsBlocked = isBlocked;
        IsDodged = isDodged;
    }

    public int Damage { get; }
    public bool IsCritical { get; }
    public bool IsBlocked { get; }
    public bool IsDodged { get; }
}

public static class NormalAttackResolver
{
    public static NormalAttackResolution Resolve(
        CharacterRuntimeStats attacker,
        CharacterRuntimeStats defender,
        float damageBoostMultiplier = 1f,
        float damageReductionMultiplier = 0f
    )
    {
        if (attacker == null || defender == null)
        {
            return new NormalAttackResolution(0, false, false, false);
        }

        bool isCritical = RollRate(attacker.CriticalRate);
        bool isBlocked = RollRate(defender.BlockRate);
        bool isDodged = RollRate(defender.DodgeRate);

        float criticalMultiplier = isCritical
            ? Mathf.Max(0f, attacker.CriticalDamage) / 100f
            : 1f;
        float defensePart = defender.Defense * (isBlocked ? 2f : 1f);
        float raw = attacker.Attack * criticalMultiplier - defensePart;
        float factor = Mathf.Max(0f, damageBoostMultiplier - damageReductionMultiplier);
        int preDodgeDamage = Mathf.Max(0, Mathf.FloorToInt(raw * factor));
        int finalDamage = isDodged ? 0 : preDodgeDamage;
        return new NormalAttackResolution(finalDamage, isCritical, isBlocked, isDodged);
    }

    private static bool RollRate(float percent)
    {
        float clamped = Mathf.Clamp(percent, 0f, 100f);
        return Random.Range(0f, 100f) <= clamped;
    }
}