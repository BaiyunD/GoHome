public sealed class ActionManagerReadyGuard : IGuardRule
{
    public bool CanExecute(ActionId actionId, ActionContext context, out string reason)
    {
        if (ActionManager.Instance != null)
        {
            reason = null;
            return true;
        }

        reason = "ActionManager.Instance is null.";
        return false;
    }
}

