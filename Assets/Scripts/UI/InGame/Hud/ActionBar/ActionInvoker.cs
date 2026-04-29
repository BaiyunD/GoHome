public sealed class ActionInvoker
{
    private readonly ActionRegistry _registry;
    private readonly CooldownService _cooldownService;
    private readonly ActionTelemetry _telemetry;

    public ActionInvoker(ActionRegistry registry, CooldownService cooldownService, ActionTelemetry telemetry)
    {
        _registry = registry;
        _cooldownService = cooldownService;
        _telemetry = telemetry;
    }

    public void Invoke(ActionId actionId, ActionContext context)
    {
        if (!TryEvaluateInternal(actionId, context, out IActionCommand command, out string reason, out bool isMissing))
        {
            if (isMissing)
            {
                _telemetry?.LogMissing(actionId, context, reason);
            }
            else
            {
                _telemetry?.LogBlocked(actionId, context, reason);
            }

            return;
        }

        _telemetry?.LogInvoked(actionId, context);
        command.Execute(context);
        _cooldownService?.MarkUsed(actionId, context);
    }

    public bool TryEvaluate(ActionId actionId, ActionContext context, out string reason)
    {
        return TryEvaluateInternal(actionId, context, out _, out reason, out _);
    }

    private bool TryEvaluateInternal(
        ActionId actionId,
        ActionContext context,
        out IActionCommand command,
        out string reason,
        out bool isMissing
    )
    {
        command = null;
        reason = null;
        isMissing = false;

        if (_registry == null)
        {
            isMissing = true;
            reason = "Registry not initialized.";
            return false;
        }

        if (!_registry.TryGetCommand(actionId, out command) || command == null)
        {
            isMissing = true;
            reason = "Command not registered.";
            return false;
        }

        ActionPolicy policy = null;
        if (_registry.TryGetPolicy(actionId, out ActionPolicy foundPolicy))
        {
            policy = foundPolicy;
        }

        if (policy != null && policy.Guards != null)
        {
            for (int i = 0; i < policy.Guards.Count; i++)
            {
                IGuardRule guard = policy.Guards[i];
                if (guard == null)
                {
                    continue;
                }

                if (!guard.CanExecute(actionId, context, out string guardReason))
                {
                    reason = ResolveReason(policy, guardReason);
                    return false;
                }
            }
        }

        if (_cooldownService != null)
        {
            float remaining = _cooldownService.GetRemainingSeconds(actionId, context);
            if (remaining > 0f)
            {
                reason = ResolveReason(policy, $"Cooldown {remaining:0.#}s");
                return false;
            }
        }

        return true;
    }

    private static string ResolveReason(ActionPolicy policy, string fallbackReason)
    {
        if (policy != null && !string.IsNullOrWhiteSpace(policy.BlockReasonKey))
        {
            return policy.BlockReasonKey;
        }

        return fallbackReason;
    }
}

