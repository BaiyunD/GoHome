using System.Collections.Generic;
using System.Text;

public static class RestLogRenderService
{
    public static string BuildItemThenTraitText(RestSettlement settlement)
    {
        if (settlement == null)
        {
            return string.Empty;
        }

        return BuildFromStructuredLogs(settlement.Logs);
    }

    private static string BuildFromStructuredLogs(List<RestLogEntry> logs)
    {
        if (logs == null || logs.Count == 0)
        {
            return string.Empty;
        }

        List<RestLogEntry> itemLogs = new List<RestLogEntry>();
        List<RestLogEntry> traitLogs = new List<RestLogEntry>();

        for (int i = 0; i < logs.Count; i++)
        {
            RestLogEntry entry = logs[i];
            if (entry.SourceType == RestLogSourceType.Item)
            {
                itemLogs.Add(entry);
            }
            else if (entry.SourceType == RestLogSourceType.Trait)
            {
                traitLogs.Add(entry);
            }
        }

        itemLogs.Sort((a, b) => a.Order.CompareTo(b.Order));
        traitLogs.Sort((a, b) => a.Order.CompareTo(b.Order));

        StringBuilder sb = new StringBuilder();
        AppendEntries(sb, itemLogs);
        AppendEntries(sb, traitLogs);
        return sb.ToString();
    }

    private static void AppendEntries(StringBuilder sb, List<RestLogEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            RestLogEntry entry = entries[i];
            if (sb.Length > 0)
            {
                sb.Append('\n');
            }

            sb.Append(RestSettlement.FormatTriggeredLine(entry.Name, entry.Description));
        }
    }
}
