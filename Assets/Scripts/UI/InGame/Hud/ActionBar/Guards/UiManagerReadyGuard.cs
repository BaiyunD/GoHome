public sealed class UiManagerReadyGuard : IGuardRule
{
    public bool CanExecute(ActionId actionId, ActionContext context, out string reason)
    {
        if (UIManager.Instance != null)
        {
            reason = null;
            return true;
        }

        reason = "UIManager.Instance is null.";
        return false;
    }
}

