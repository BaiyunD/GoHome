public sealed class AdvanceCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        ActionManager.Instance.TryAdvance();
    }
}

