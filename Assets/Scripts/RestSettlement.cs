using System.Collections.Generic;
using UnityEngine;

public enum RestLogSourceType
{
    Item = 0,
    Trait = 1,
    Summary = 2,
    TemporaryState = 3,
    DayEnd = 4,
    DayStart = 5
}

public struct RestLogEntry
{
    public RestLogSourceType SourceType;
    public string Name;
    public string Description;
    public int Order;
}

public class RestSettlement
{
    private int _nextLogOrder;

    public int DayBefore
    {
        get; private set;
    }

    public int DayAfter
    {
        get; private set;
    }

    public int EnergyBefore
    {
        get; private set;
    }

    public int EnergyAfter
    {
        get; private set;
    }

    public int EnergyMaxAfter
    {
        get; private set;
    }

    public int HungerBefore
    {
        get; private set;
    }

    public int HungerAfter
    {
        get; private set;
    }

    public int HungerMaxAfter
    {
        get; private set;
    }

    public int HPBefore
    {
        get; private set;
    }

    public int HPAfter
    {
        get; private set;
    }

    public int HPMaxAfter
    {
        get; private set;
    }

    public int DayDelta => DayAfter - DayBefore;
    public int EnergyDelta => EnergyAfter - EnergyBefore;
    public int HungerDelta => HungerAfter - HungerBefore;
    public int HPDelta => HPAfter - HPBefore;
    public int DisplayedEnergyDelta
    {
        get; private set;
    }

    public int DisplayedHungerDelta
    {
        get; private set;
    }

    public int DisplayedHPDelta
    {
        get; private set;
    }

    public List<string> SummaryLines
    {
        get; private set;
    } = new List<string>();

    public List<string> TemporaryStateLines
    {
        get; private set;
    } = new List<string>();

    public List<string> DayEndLines
    {
        get; private set;
    } = new List<string>();

    public List<string> DayStartLines
    {
        get; private set;
    } = new List<string>();

    public List<RestLogEntry> Logs
    {
        get; private set;
    } = new List<RestLogEntry>();

    public static RestSettlement CreateSnapshot()
    {
        RestSettlement settlement = new RestSettlement();
        settlement.CaptureBefore();
        settlement.CaptureAfter();
        return settlement;
    }

    public void CaptureBefore()
    {
        DayBefore = RouteProgressManager.Instance != null ? RouteProgressManager.Instance.GetDay() : 0;
        EnergyBefore = TryGetResourceInt(PlayerResourceType.Energy);
        HungerBefore = TryGetResourceInt(PlayerResourceType.Hunger);
        HPBefore = TryGetResourceInt(PlayerResourceType.HP);
    }

    public void CaptureAfter()
    {
        DayAfter = RouteProgressManager.Instance != null ? RouteProgressManager.Instance.GetDay() : 0;
        EnergyAfter = TryGetResourceInt(PlayerResourceType.Energy);
        HungerAfter = TryGetResourceInt(PlayerResourceType.Hunger);
        HPAfter = TryGetResourceInt(PlayerResourceType.HP);

        EnergyMaxAfter = TryGetResourceMaxInt(PlayerResourceType.Energy);
        HungerMaxAfter = TryGetResourceMaxInt(PlayerResourceType.Hunger);
        HPMaxAfter = TryGetResourceMaxInt(PlayerResourceType.HP);
    }

    public void ApplyRestContext(RestContext restContext)
    {
        if (restContext == null)
        {
            return;
        }

        DisplayedEnergyDelta = restContext.DisplayedEnergyRecover;
        DisplayedHungerDelta = restContext.DisplayedHungerDelta;
        DisplayedHPDelta = restContext.DisplayedHpRecover;
    }

    public void AddSummaryLine(string line)
    {
        AddLine(SummaryLines, line);
    }

    public void AddTemporaryStateLine(string line)
    {
        AddLine(TemporaryStateLines, line);
    }

    public void AddDayEndLine(string line)
    {
        AddLine(DayEndLines, line);
    }

    public void AddDayStartLine(string line)
    {
        AddLine(DayStartLines, line);
    }

    public static string FormatTriggeredLine(string name, string description)
    {
        string safeName = string.IsNullOrWhiteSpace(name) ? "null" : name;
        string safeDescription = string.IsNullOrWhiteSpace(description) ? "null" : description;
        return $"【{safeName}】{safeDescription}";
    }

    public void AddLog(RestLogSourceType sourceType, string name, string description)
    {
        Logs.Add(new RestLogEntry
        {
            SourceType = sourceType,
            Name = NormalizeNullable(name),
            Description = NormalizeNullable(description),
            Order = _nextLogOrder++
        });
    }

    private static string NormalizeNullable(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "null" : value;
    }

    private static void AddLine(List<string> lines, string line)
    {
        if (lines == null || string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        lines.Add(line);
    }

    public static int TryGetResourceInt(PlayerResourceType type)
    {
        if (PlayerResourceService.Instance != null &&
            PlayerResourceService.Instance.TryGetValue(type, out float value))
        {
            return Mathf.RoundToInt(value);
        }

        return 0;
    }

    public static int TryGetResourceMaxInt(PlayerResourceType type)
    {
        if (PlayerResourceService.Instance != null &&
            PlayerResourceService.Instance.TryGetMaxValue(type, out float value))
        {
            return Mathf.RoundToInt(value);
        }

        return 0;
    }
}
