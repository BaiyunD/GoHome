using System.Collections.Generic;

public enum CombatActionEffectType
{
    DamageEnemy = 0,
    DamagePlayer = 1
}

public sealed class CombatActionEffect
{
    public CombatActionEffect(CombatActionEffectType effectType, int amount)
    {
        EffectType = effectType;
        Amount = amount;
    }

    public CombatActionEffectType EffectType { get; }
    public int Amount { get; }
}

public enum CombatSettlementLogType
{
    Attack = 0,
    Hint = 1
}

public sealed class CombatSettlementLog
{
    private CombatSettlementLog(CombatSettlementLogType logType, BattleAttackEvent attackEvent, string message)
    {
        LogType = logType;
        AttackEvent = attackEvent;
        Message = message ?? string.Empty;
    }

    public CombatSettlementLogType LogType { get; }
    public BattleAttackEvent AttackEvent { get; }
    public string Message { get; }

    public static CombatSettlementLog FromAttack(BattleAttackEvent attackEvent)
    {
        return new CombatSettlementLog(CombatSettlementLogType.Attack, attackEvent, string.Empty);
    }

    public static CombatSettlementLog FromHint(string message)
    {
        return new CombatSettlementLog(CombatSettlementLogType.Hint, null, message);
    }
}

public sealed class CombatActionResult
{
    public List<CombatActionEffect> Effects { get; } = new List<CombatActionEffect>();
    public List<CombatSettlementLog> SettlementLogs { get; } = new List<CombatSettlementLog>();
    public BattleResult EndIntent { get; set; } = BattleResult.None;
}
