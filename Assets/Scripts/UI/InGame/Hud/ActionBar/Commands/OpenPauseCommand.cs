public sealed class OpenPauseCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        UIManager.Instance.OpenUIEntry(UIKey.PausePage);
    }
}

