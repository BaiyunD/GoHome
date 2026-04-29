using UnityEngine;

public sealed class ActionContext
{
    public ActionContext(MonoBehaviour sender, string source)
    {
        Sender = sender;
        Source = source;
    }

    public MonoBehaviour Sender { get; }
    public string Source { get; }
}

