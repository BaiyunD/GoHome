public sealed class OpenInventoryCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        UIManager.Instance.OpenUIEntry(UIKey.InventoryPage);
    }
}

