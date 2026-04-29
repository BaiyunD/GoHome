using System.Collections.Generic;

public sealed class ActionRegistry
{
    private readonly Dictionary<ActionId, IActionCommand> _commands = new Dictionary<ActionId, IActionCommand>();
    private readonly Dictionary<ActionId, ActionPolicy> _policies = new Dictionary<ActionId, ActionPolicy>();

    public void Register(ActionId actionId, IActionCommand command, ActionPolicy policy)
    {
        _commands[actionId] = command;
        _policies[actionId] = policy;
    }

    public bool TryGetCommand(ActionId actionId, out IActionCommand command)
    {
        return _commands.TryGetValue(actionId, out command);
    }

    public bool TryGetPolicy(ActionId actionId, out ActionPolicy policy)
    {
        return _policies.TryGetValue(actionId, out policy);
    }
}

