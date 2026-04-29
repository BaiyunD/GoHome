public sealed class CooldownService
{
    public float GetRemainingSeconds(ActionId actionId, ActionContext context)
    {
        return 0f;
    }

    public void MarkUsed(ActionId actionId, ActionContext context)
    {
    }
}

