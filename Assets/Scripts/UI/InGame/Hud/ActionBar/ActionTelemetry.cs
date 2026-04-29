using UnityEngine;

public sealed class ActionTelemetry
{
    public void LogInvoked(ActionId actionId, ActionContext context)
    {
        Debug.LogFormat("[ActionBar] Invoke {0} (Source: {1})", actionId, context != null ? context.Source : "null");
    }

    public void LogBlocked(ActionId actionId, ActionContext context, string reason)
    {
        Debug.LogWarningFormat(
            "[ActionBar] Blocked {0} (Source: {1}) Reason: {2}",
            actionId,
            context != null ? context.Source : "null",
            string.IsNullOrWhiteSpace(reason) ? "Unknown" : reason
        );
    }

    public void LogMissing(ActionId actionId, ActionContext context, string message)
    {
        Debug.LogErrorFormat(
            "[ActionBar] Missing {0} (Source: {1}) {2}",
            actionId,
            context != null ? context.Source : "null",
            string.IsNullOrWhiteSpace(message) ? string.Empty : message
        );
    }
}

