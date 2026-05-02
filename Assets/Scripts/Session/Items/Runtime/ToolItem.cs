using UnityEngine;

public abstract class ToolItem : ItemBase
{
    public virtual void ApplyPassive(PassiveAccumulator accumulator, int level)
    {
    }

    public virtual void OnRest(RestContext context, int level)
    {
    }
}

public class PassiveAccumulator
{
    public float HpMaxBonus;
    public float EnergyMaxBonus;
    public float HungerMaxBonus;

    public float AttackBonus;
    public float DefenseBonus;

    public float CriticalRateBonus;
    public float BlockRateBonus;
    public float DodgeRateBonus;
    public float CriticalDamageBonus;
    public float EscapeRateBonus;
}

public class RestContext
{
    public GameManager GameManager
    {
        get;
    }

    public int DisplayedHpRecover
    {
        get; set;
    }

    public int DisplayedHungerDelta
    {
        get; set;
    }

    public int DisplayedEnergyRecover
    {
        get; set;
    }

    public RestSettlement Settlement
    {
        get;
        set;
    }

    public int CurrentRestItemId
    {
        get;
        set;
    }

    public string CurrentRestItemDisplayName
    {
        get;
        set;
    }

    public RestContext(GameManager gameManager)
    {
        GameManager = gameManager;
    }

    public void AddTraitTriggeredLog(string traitName, string description)
    {
        Settlement?.AddLog(RestLogSourceType.Trait, traitName, description);
    }

    public void AddItemTriggeredLog(string itemName, string description)
    {
        Settlement?.AddLog(RestLogSourceType.Item, itemName, description);
    }

    public void AddTemporaryStateLine(string line)
    {
        Settlement?.AddTemporaryStateLine(line);
    }

    public void AddSummaryLine(string line)
    {
        Settlement?.AddSummaryLine(line);
    }
}

