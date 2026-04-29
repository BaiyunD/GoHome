using System.Collections.Generic;
using UnityEngine;

public sealed class EnergySurplusRestPrecheckHandler : IRestPrecheckHandler
{
    private const string DEFAULT_HINT = "当前还有富余体力哦~还是想要休息的话再来吧";

    public RestPrecheckResult Evaluate(RestFlowContext context)
    {
        if (PlayerResourceService.Instance == null)
        {
            return new RestPrecheckResult(RestPrecheckDecision.Reject, "玩家资源服务未就绪，暂时无法休息。");
        }

        if (!PlayerResourceService.Instance.TryGetValue(PlayerResourceType.Energy, out float energyValue))
        {
            return new RestPrecheckResult(RestPrecheckDecision.Reject, "精力数据暂不可用，暂时无法休息。");
        }

        if (energyValue <= 0f || context.HasConfirmedEnergySurplus)
        {
            return RestPrecheckResult.Pass;
        }

        return new RestPrecheckResult(RestPrecheckDecision.NeedSecondClick, DEFAULT_HINT);
    }
}

public sealed class DefaultRestSettlementHandler : IRestSettlementHandler
{
    public void Execute(RestFlowContext context)
    {
        if (context == null)
        {
            return;
        }

        context.Settlement.CaptureBefore();

        GlobalRulesConfig rules = AppRoot.Instance != null && AppRoot.Instance.Configs != null
            ? AppRoot.Instance.Configs.GlobalRules
            : null;
        float restEnergyPercent = rules != null ? Mathf.Max(0f, rules.restEnergyPercent) : 1f;
        int restHpDelta = rules != null ? rules.restHPDelta : 0;
        int restHungerDelta = rules != null ? rules.restHungerDelta : -20;

        int energyMaxCurrent = RestSettlement.TryGetResourceMaxInt(PlayerResourceType.Energy);
        int energyCurrent = RestSettlement.TryGetResourceInt(PlayerResourceType.Energy);
        int displayedEnergyRecover = Mathf.Max(
            0,
            Mathf.RoundToInt(energyMaxCurrent * restEnergyPercent) - energyCurrent);

        RestContext restContext = new RestContext(context.GameManager)
        {
            DisplayedEnergyRecover = displayedEnergyRecover,
            DisplayedHungerDelta = restHungerDelta,
            DisplayedHpRecover = restHpDelta,
            Settlement = context.Settlement
        };

        context.RestContext = restContext;

        if (RouteProgressManager.Instance != null)
        {
            RouteProgressManager.Instance.Rest(1);
            context.Settlement.AddSummaryLine("天数 +1");
        }

        if (displayedEnergyRecover != 0)
        {
            context.Settlement.AddSummaryLine($"基础体力恢复 {displayedEnergyRecover}");
        }

        if (restHpDelta != 0)
        {
            context.Settlement.AddSummaryLine($"基础生命变化 {restHpDelta:+#;-#;0}");
        }

        if (restHungerDelta != 0)
        {
            context.Settlement.AddSummaryLine($"基础饥饿变化 {restHungerDelta:+#;-#;0}");
        }
    }
}

public sealed class ItemRestSettlementHandler : IRestSettlementHandler
{
    public void Execute(RestFlowContext context)
    {
        if (context == null || context.RestContext == null || ItemRegistry.Instance == null)
        {
            return;
        }

        // Snapshot keys first: item effects may modify inventory during rest settlement.
        List<int> itemIds = new List<int>();
        foreach (int itemId in InventoryManager.inventoryDict.Keys)
        {
            itemIds.Add(itemId);
        }

        itemIds.Sort();
        for (int i = 0; i < itemIds.Count; i++)
        {
            int itemId = itemIds[i];
            int level = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(itemId) : 0;
            if (level <= 0)
            {
                continue;
            }

            if (!ItemRegistry.Instance.TryGet(itemId, out ItemBase item) || item == null)
            {
                continue;
            }

            ToolItem tool = item as ToolItem;
            if (tool != null)
            {
                tool.OnRest(context.RestContext, level);
            }

            ItemEffectDispatcher.OnRestItemEffect(itemId, level, context.RestContext, out ItemEffectSource _);
        }
    }
}

public sealed class TraitRestSettlementHandler : IRestSettlementHandler
{
    public void Execute(RestFlowContext context)
    {
        if (context == null || context.RestContext == null || TraitManager.Instance == null)
        {
            return;
        }

        TraitManager.Instance.ApplyRestSettlement(context.RestContext);
    }
}

public sealed class TemporaryStateRestSettlementHandler : IRestSettlementHandler
{
    public void Execute(RestFlowContext context)
    {
    }
}

public sealed class FinalizeRestSettlementHandler : IRestSettlementHandler
{
    public void Execute(RestFlowContext context)
    {
        if (context == null || context.RestContext == null)
        {
            return;
        }

        int energyMaxCurrent = RestSettlement.TryGetResourceMaxInt(PlayerResourceType.Energy);
        int energyBefore = RestSettlement.TryGetResourceInt(PlayerResourceType.Energy);
        int targetEnergy = Mathf.Clamp(
            energyBefore + context.RestContext.DisplayedEnergyRecover,
            0,
            energyMaxCurrent);
        int actualEnergyRecover = targetEnergy - energyBefore;
        context.RestContext.DisplayedEnergyRecover = actualEnergyRecover;

        if (PlayerResourceService.Instance != null)
        {
            PlayerResourceService.Instance.TrySetValue(
                PlayerResourceType.Energy,
                targetEnergy,
                "RestSettlement.Rest");
            PlayerResourceService.Instance.ApplyDelta(
                PlayerResourceType.HP,
                context.RestContext.DisplayedHpRecover,
                "RestSettlement.Rest");
            PlayerResourceService.Instance.ApplyDelta(
                PlayerResourceType.Hunger,
                context.RestContext.DisplayedHungerDelta,
                "RestSettlement.Rest");
        }

        context.Settlement.ApplyRestContext(context.RestContext);
        context.Settlement.CaptureAfter();
    }
}

public sealed class DailyItemDayStartHandler : IDayStartHandler
{
    public RestFlowDirective Execute(RestFlowContext context)
    {
        // DayStart is reserved for story + custom hooks only.
        return RestFlowDirective.Continue;
    }
}

public sealed class TraitDayStartHandler : IDayStartHandler
{
    public RestFlowDirective Execute(RestFlowContext context)
    {
        return RestFlowDirective.Continue;
    }
}

public sealed class PlaceholderDayEndHandler : IDayEndHandler
{
    public RestFlowDirective Execute(RestFlowContext context)
    {
        return RestFlowDirective.Continue;
    }
}

public sealed class PlaceholderDayStartEventHandler : IDayStartHandler
{
    public RestFlowDirective Execute(RestFlowContext context)
    {
        return RestFlowDirective.Continue;
    }
}
