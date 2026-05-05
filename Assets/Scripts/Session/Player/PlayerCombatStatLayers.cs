using UnityEngine;

/// <summary>
/// 角色自身战斗面板（唯一持久化层；战斗奖励与剧情永久成长写这里）。
/// </summary>
[System.Serializable]
public struct PlayerCombatStatBase
{
    public float MaxHp;
    public float Attack;
    public float Defense;
    public float CriticalRate;
    public float CriticalDamage;
    public float BlockRate;
    public float DodgeRate;
    public float EscapeRate;

    public static PlayerCombatStatBase FromPlayerData(PlayerData data)
    {
        if (data == null)
        {
            return default;
        }

        return new PlayerCombatStatBase
        {
            MaxHp = Mathf.Max(0f, data.HP),
            Attack = Mathf.Max(0f, data.Attack),
            Defense = Mathf.Max(0f, data.Defense),
            CriticalRate = data.CriticalRate,
            CriticalDamage = Mathf.Max(0f, data.CriticalDamage),
            BlockRate = data.BlockRate,
            DodgeRate = data.DodgeRate,
            EscapeRate = data.EscapeRate,
        };
    }
}

/// <summary>
/// 道具被动层（运行时；由背包/合成事件重算；默认不进存档）。
/// </summary>
[System.Serializable]
public struct PlayerCombatStatItemPassive
{
    public float MaxHpBonus;
    public float AttackBonus;
    public float DefenseBonus;
    public float CriticalRateBonus;
    public float CriticalDamageBonus;
    public float BlockRateBonus;
    public float DodgeRateBonus;
    public float EscapeRateBonus;

    public static PlayerCombatStatItemPassive FromAccumulator(PassiveAccumulator acc)
    {
        if (acc == null)
        {
            return default;
        }

        return new PlayerCombatStatItemPassive
        {
            MaxHpBonus = acc.HpMaxBonus,
            AttackBonus = acc.AttackBonus,
            DefenseBonus = acc.DefenseBonus,
            CriticalRateBonus = acc.CriticalRateBonus,
            CriticalDamageBonus = acc.CriticalDamageBonus,
            BlockRateBonus = acc.BlockRateBonus,
            DodgeRateBonus = acc.DodgeRateBonus,
            EscapeRateBonus = acc.EscapeRateBonus,
        };
    }
}

/// <summary>
/// 展示与战斗使用的最终战斗数值（只读快照；由 Base + ItemPassive 计算）。
/// </summary>
public readonly struct PlayerCombatStatFinal
{
    public PlayerCombatStatFinal(
        float maxHp,
        float attack,
        float defense,
        float criticalRate,
        float criticalDamage,
        float blockRate,
        float dodgeRate,
        float escapeRate)
    {
        MaxHp = maxHp;
        Attack = attack;
        Defense = defense;
        CriticalRate = criticalRate;
        CriticalDamage = criticalDamage;
        BlockRate = blockRate;
        DodgeRate = dodgeRate;
        EscapeRate = escapeRate;
    }

    public float MaxHp { get; }
    public float Attack { get; }
    public float Defense { get; }
    public float CriticalRate { get; }
    public float CriticalDamage { get; }
    public float BlockRate { get; }
    public float DodgeRate { get; }
    public float EscapeRate { get; }
}

/// <summary>
/// 由 Base 与道具被动层合成最终战斗面板（唯一推荐入口）。
/// </summary>
public static class PlayerCombatStatCalculator
{
    public const int SaveFormatCombatLayers = 4;

    public static PlayerCombatStatFinal Combine(PlayerCombatStatBase b, PlayerCombatStatItemPassive p)
    {
        float maxHp = Mathf.Max(0f, b.MaxHp + p.MaxHpBonus);
        float attack = Mathf.Max(0f, b.Attack + p.AttackBonus);
        float defense = Mathf.Max(0f, b.Defense + p.DefenseBonus);
        float criticalRate = CharacterDataBase.ClampRate(b.CriticalRate + p.CriticalRateBonus);
        float criticalDamage = Mathf.Max(0f, b.CriticalDamage + p.CriticalDamageBonus);
        float blockRate = CharacterDataBase.ClampRate(b.BlockRate + p.BlockRateBonus);
        float dodgeRate = CharacterDataBase.ClampRate(b.DodgeRate + p.DodgeRateBonus);
        float escapeRate = CharacterDataBase.ClampRate(b.EscapeRate + p.EscapeRateBonus);
        return new PlayerCombatStatFinal(
            maxHp,
            attack,
            defense,
            criticalRate,
            criticalDamage,
            blockRate,
            dodgeRate,
            escapeRate);
    }
}
