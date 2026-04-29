public sealed class OpenTraitsCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        UIManager.Instance.OpenUIEntry(UIKey.TraitsPage);
        UIManager.Instance.UpdateInfo();
    }
}

