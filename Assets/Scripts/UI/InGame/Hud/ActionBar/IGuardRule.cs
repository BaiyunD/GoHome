public interface IGuardRule
{
    bool CanExecute(ActionId actionId, ActionContext context, out string reason);
}

