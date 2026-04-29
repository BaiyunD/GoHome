public sealed class OpenCraftCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        UIManager.Instance.OpenUIEntry(UIKey.CraftPage);
    }
}

