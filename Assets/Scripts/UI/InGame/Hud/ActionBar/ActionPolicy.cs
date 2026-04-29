using System.Collections.Generic;

public sealed class ActionPolicy
{
    public ActionPolicy(
        List<IGuardRule> guards,
        float cooldownSeconds = 0f,
        string blockReasonKey = null
    )
    {
        Guards = guards;
        CooldownSeconds = cooldownSeconds;
        BlockReasonKey = blockReasonKey;
    }

    public List<IGuardRule> Guards { get; }
    public float CooldownSeconds { get; }
    public string BlockReasonKey { get; }
}

