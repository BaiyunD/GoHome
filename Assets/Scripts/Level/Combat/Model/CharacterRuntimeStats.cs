using System.Collections.Generic;
using UnityEngine;

public sealed class CharacterRuntimeStats
{
    public CharacterRuntimeStats(CharacterDataBase template)
    {
        if (template == null)
        {
            Name = string.Empty;
            MaxHp = 0;
            CurrentHp = 0;
            Attack = 0;
            Defense = 0;
            CriticalRate = 0f;
            CriticalDamage = 0f;
            DodgeRate = 0f;
            BlockRate = 0f;
            EscapeRate = 0f;
            TraitIds = new List<string>();
            return;
        }

        Name = template.CharacterName;
        MaxHp = Mathf.Max(0, template.HP);
        CurrentHp = MaxHp;
        Attack = Mathf.Max(0, template.Attack);
        Defense = Mathf.Max(0, template.Defense);
        CriticalRate = CharacterDataBase.ClampRate(template.CriticalRate);
        CriticalDamage = Mathf.Max(0f, template.CriticalDamage);
        DodgeRate = CharacterDataBase.ClampRate(template.DodgeRate);
        BlockRate = CharacterDataBase.ClampRate(template.BlockRate);
        EscapeRate = CharacterDataBase.ClampRate(template.EscapeRate);
        TraitIds = new List<string>(template.TraitIds);
    }

    public CharacterRuntimeStats(PlayerRuntime playerRuntime, IReadOnlyList<string> traitIds = null, float escapeRate = 0f)
    {
        Name = playerRuntime != null ? playerRuntime.DisplayName : "Player";
        MaxHp = playerRuntime != null ? Mathf.Max(0, Mathf.RoundToInt(playerRuntime.MaxHp)) : 0;
        CurrentHp = playerRuntime != null
            ? Mathf.Clamp(Mathf.RoundToInt(playerRuntime.CurrentHp), 0, MaxHp)
            : 0;
        Attack = playerRuntime != null
            ? Mathf.Max(0, Mathf.RoundToInt(playerRuntime.Attack))
            : 0;
        Defense = playerRuntime != null
            ? Mathf.Max(0, Mathf.RoundToInt(playerRuntime.Defense))
            : 0;

        CriticalRate = playerRuntime != null
            ? CharacterDataBase.ClampRate(playerRuntime.CriticalRate)
            : 0f;
        CriticalDamage = playerRuntime != null
            ? Mathf.Max(0f, playerRuntime.CriticalDamage)
            : 0f;
        DodgeRate = playerRuntime != null
            ? CharacterDataBase.ClampRate(playerRuntime.DodgeRate)
            : 0f;
        BlockRate = playerRuntime != null
            ? CharacterDataBase.ClampRate(playerRuntime.BlockRate)
            : 0f;
        EscapeRate = CharacterDataBase.ClampRate(escapeRate);
        TraitIds = traitIds != null ? new List<string>(traitIds) : new List<string>();
    }

    public CharacterRuntimeStats(EnemyRuntime enemyRuntime)
    {
        Name = enemyRuntime != null ? enemyRuntime.DisplayName : string.Empty;
        MaxHp = enemyRuntime != null ? Mathf.Max(0, Mathf.RoundToInt(enemyRuntime.MaxHp)) : 0;
        CurrentHp = enemyRuntime != null
            ? Mathf.Clamp(Mathf.RoundToInt(enemyRuntime.CurrentHp), 0, MaxHp)
            : 0;
        Attack = enemyRuntime != null ? Mathf.Max(0, Mathf.RoundToInt(enemyRuntime.Attack)) : 0;
        Defense = enemyRuntime != null ? Mathf.Max(0, Mathf.RoundToInt(enemyRuntime.Defense)) : 0;
        CriticalRate = enemyRuntime != null ? CharacterDataBase.ClampRate(enemyRuntime.CriticalRate) : 0f;
        CriticalDamage = enemyRuntime != null ? Mathf.Max(0f, enemyRuntime.CriticalDamage) : 0f;
        DodgeRate = enemyRuntime != null ? CharacterDataBase.ClampRate(enemyRuntime.DodgeRate) : 0f;
        BlockRate = enemyRuntime != null ? CharacterDataBase.ClampRate(enemyRuntime.BlockRate) : 0f;
        EscapeRate = enemyRuntime != null
            ? CharacterDataBase.ClampRate(enemyRuntime.EscapeRate)
            : 0f;
        TraitIds = enemyRuntime != null && enemyRuntime.TraitIds != null
            ? new List<string>(enemyRuntime.TraitIds)
            : new List<string>();
    }

    public string Name { get; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public float CriticalRate { get; private set; }
    public float CriticalDamage { get; private set; }
    public float DodgeRate { get; private set; }
    public float BlockRate { get; private set; }
    public float EscapeRate { get; private set; }
    public List<string> TraitIds { get; }

    public void ApplyDamage(int damage)
    {
        if (damage <= 0)
        {
            damage = 0;
        }

        CurrentHp = Mathf.Max(0, CurrentHp - damage);
    }

    public void Heal(int value)
    {
        if (value <= 0)
        {
            return;
        }

        CurrentHp = Mathf.Min(MaxHp, CurrentHp + value);
    }

    public void SetCurrentHp(int value)
    {
        CurrentHp = Mathf.Clamp(value, 0, MaxHp);
    }

    public void AddDefenseModifier(int delta)
    {
        Defense = Mathf.Max(0, Defense + delta);
    }

    public void AddEscapeRateModifier(float deltaPercent)
    {
        EscapeRate = CharacterDataBase.ClampRate(EscapeRate + deltaPercent);
    }
}
