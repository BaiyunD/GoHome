using System.Collections.Generic;

public static class ActionBarBootstrap
{
    public static ActionRegistry BuildRegistry()
    {
        ActionRegistry registry = new ActionRegistry();

        IGuardRule notInBattle = new NotInBattleGuard();
        IGuardRule actionManagerReady = new ActionManagerReadyGuard();
        IGuardRule uiManagerReady = new UiManagerReadyGuard();

        registry.Register(
            ActionId.Advance,
            new AdvanceCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, actionManagerReady })
        );
        registry.Register(
            ActionId.Explore,
            new ExploreCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, actionManagerReady })
        );
        registry.Register(
            ActionId.Rest,
            new RestCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, actionManagerReady })
        );
        registry.Register(
            ActionId.Inventory,
            new OpenInventoryCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, uiManagerReady })
        );
        registry.Register(
            ActionId.Craft,
            new OpenCraftCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, uiManagerReady })
        );
        registry.Register(
            ActionId.Traits,
            new OpenTraitsCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, uiManagerReady })
        );
        registry.Register(
            ActionId.Pause,
            new OpenPauseCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, uiManagerReady })
        );
        registry.Register(
            ActionId.Shop,
            new OpenShopCommand(),
            new ActionPolicy(new List<IGuardRule> { notInBattle, uiManagerReady })
        );

        return registry;
    }

    public static ActionInvoker BuildInvoker()
    {
        ActionRegistry registry = BuildRegistry();
        ActionTelemetry telemetry = new ActionTelemetry();
        CooldownService cooldownService = new CooldownService();

        return new ActionInvoker(registry, cooldownService, telemetry);
    }
}
