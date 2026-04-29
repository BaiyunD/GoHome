public sealed class RestCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        ActionManager.Instance.TryRest();
    }
}

