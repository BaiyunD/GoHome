public sealed class OpenShopCommand : IActionCommand
{
    public void Execute(ActionContext context)
    {
        UIManager.Instance.OpenUIEntry(UIKey.ShopPage);
    }
}
