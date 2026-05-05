using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyRuntime
{
    public EnemyData RuntimeData
    {
        get; private set;
    }

    public string EnemyId { get; private set; }
    public int Level { get; private set; }
    public EnemyKind Kind { get; private set; }
    public string DisplayName { get; private set; }
    public float EscapeRate { get; private set; }
    public bool CanEscape { get; private set; }
    public IReadOnlyList<string> TraitIds { get; private set; }

    public float CurrentHp { get; private set; }
    public float MaxHp { get; set; }
    public float Attack { get; set; }
    public float Defense { get; set; }
    public float CriticalRate { get; set; }
    public float CriticalDamage { get; set; }
    public float BlockRate { get; set; }
    public float DodgeRate { get; set; }

    public EnemyRuntime(EnemyData runtimeData)
    {
        RuntimeData = runtimeData;
        ResetFromTemplate(runtimeData);
    }

    public void ResetFromTemplate(EnemyData enemyTemplate)
    {
        RuntimeData = enemyTemplate;
        EnemyData data = RuntimeData;
        EnemyId = data != null ? data.EnemyId : string.Empty;
        Level = data != null ? data.Level : 1;
        Kind = data != null ? data.Kind : EnemyKind.Minion;
        DisplayName = data != null ? data.CharacterName : string.Empty;
        EscapeRate = data != null ? data.EscapeRate : 0f;
        CanEscape = data != null && data.CanEscape;
        TraitIds = data != null ? data.TraitIds : new List<string>();

        MaxHp = data != null ? data.HP : 100f;
        CurrentHp = MaxHp;
        Attack = data != null ? data.Attack : 0f;
        Defense = data != null ? data.Defense : 0f;
        CriticalRate = data != null ? data.CriticalRate : 0f;
        CriticalDamage = data != null ? data.CriticalDamage : 150f;
        BlockRate = data != null ? data.BlockRate : 0f;
        DodgeRate = data != null ? data.DodgeRate : 0f;
    }

    public void SetCurrentHp(float value)
    {
        float safeMax = Mathf.Max(0f, MaxHp);
        CurrentHp = Mathf.Clamp(value, 0f, safeMax);
    }
}
