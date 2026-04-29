public sealed class ExploreCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        ActionManager.Instance.TryExplore();
    }
}

