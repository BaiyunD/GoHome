using System;
using System.Collections.Generic;
using UnityEngine;

public class RestManager : MonoBehaviour
{
    private readonly List<IRestPrecheckHandler> _precheckHandlers = new List<IRestPrecheckHandler>();
    private readonly List<IDayEndHandler> _dayEndHandlers = new List<IDayEndHandler>();
    private readonly List<IRestSettlementHandler> _restSettlementHandlers = new List<IRestSettlementHandler>();
    private readonly List<IDayStartHandler> _dayStartHandlers = new List<IDayStartHandler>();
    private RestFlowContext _storyPausedFlowContext;
    private RestFlowContext _waitForPlayerConfirmContext;
    private readonly RestStoryBusGateway _storyBusGateway = new RestStoryBusGateway();
    private bool _hasConfirmedEnergySurplus;

    public static RestManager Instance
    {
        get; private set;
    }

    public event Action<RestSettlement> OnRestSettled;

    private void Awake()
    {
        Instance = this;
        RegisterDefaultHandlers();
    }

    public RestBeginResult TryBeginRest()
    {
        if (GameManager.Instance == null || UIManager.Instance == null)
        {
            Debug.LogWarning("RestManager.TryBeginRest -> GameManager or UIManager is null");
            return RestBeginResult.Rejected;
        }

        if (_storyPausedFlowContext != null || _waitForPlayerConfirmContext != null)
        {
            Debug.LogWarning("RestManager.TryBeginRest -> 当前已有休息流程在进行中。");
            return RestBeginResult.Rejected;
        }

        RestFlowContext context = new RestFlowContext(GameManager.Instance, _hasConfirmedEnergySurplus);
        RestPrecheckResult precheckResult = EvaluatePrechecks(context);
        if (precheckResult.Decision == RestPrecheckDecision.NeedSecondClick)
        {
            context.BlockMessage = precheckResult.Message;
            _hasConfirmedEnergySurplus = true;
            return RestBeginResult.NeedSecondClick;
        }

        if (precheckResult.Decision == RestPrecheckDecision.Reject)
        {
            context.BlockMessage = precheckResult.Message;
            if (!string.IsNullOrWhiteSpace(precheckResult.Message))
            {
                UIManager.Instance.ShowEventNarrationText(precheckResult.Message);
            }

            _hasConfirmedEnergySurplus = false;
            return RestBeginResult.Rejected;
        }

        _hasConfirmedEnergySurplus = false;
        return AdvanceFlow(context, RestFlowStage.DayEnd);
    }

    public void RegisterStoryRequestListener(Action<RestStoryBusRequest> listener)
    {
        if (listener == null)
        {
            return;
        }

        _storyBusGateway.StoryRequested -= listener;
        _storyBusGateway.StoryRequested += listener;
    }

    public void UnregisterStoryRequestListener(Action<RestStoryBusRequest> listener)
    {
        if (listener == null)
        {
            return;
        }

        _storyBusGateway.StoryRequested -= listener;
    }

    public void CompleteRestStoryPause()
    {
        _storyBusGateway.CompleteCurrentStory();
    }

    public void ConfirmRest()
    {
        if (_waitForPlayerConfirmContext == null)
        {
            return;
        }

        RestFlowContext context = _waitForPlayerConfirmContext;
        _waitForPlayerConfirmContext = null;

        UIManager.Instance.CloseUIEntry(UIKey.RestPage);
        AdvanceFlow(context, RestFlowStage.DayStart);
    }

    public void RegisterPrecheckHandler(IRestPrecheckHandler handler)
    {
        RegisterHandler(_precheckHandlers, handler);
    }

    public void RegisterDayEndHandler(IDayEndHandler handler)
    {
        RegisterHandler(_dayEndHandlers, handler);
    }

    public void RegisterRestSettlementHandler(IRestSettlementHandler handler)
    {
        RegisterHandler(_restSettlementHandlers, handler);
    }

    public void RegisterDayStartHandler(IDayStartHandler handler)
    {
        RegisterHandler(_dayStartHandlers, handler);
    }

    private void RegisterDefaultHandlers()
    {
        if (_precheckHandlers.Count > 0 ||
            _dayEndHandlers.Count > 0 ||
            _restSettlementHandlers.Count > 0 ||
            _dayStartHandlers.Count > 0)
        {
            return;
        }

        _precheckHandlers.Add(new EnergySurplusRestPrecheckHandler());
        _dayEndHandlers.Add(new PlaceholderDayEndHandler());
        _restSettlementHandlers.Add(new DefaultRestSettlementHandler());
        _restSettlementHandlers.Add(new ItemRestSettlementHandler());
        _restSettlementHandlers.Add(new TraitRestSettlementHandler());
        _restSettlementHandlers.Add(new TemporaryStateRestSettlementHandler());
        _restSettlementHandlers.Add(new FinalizeRestSettlementHandler());
        _dayStartHandlers.Add(new DailyItemDayStartHandler());
        _dayStartHandlers.Add(new TraitDayStartHandler());
        _dayStartHandlers.Add(new PlaceholderDayStartEventHandler());
    }

    private RestPrecheckResult EvaluatePrechecks(RestFlowContext context)
    {
        context.CurrentStage = RestFlowStage.Precheck;
        for (int i = 0; i < _precheckHandlers.Count; i++)
        {
            RestPrecheckResult result = _precheckHandlers[i].Evaluate(context);
            if (result == null || result.Decision == RestPrecheckDecision.Pass)
            {
                continue;
            }

            return result;
        }

        return RestPrecheckResult.Pass;
    }

    private RestBeginResult AdvanceFlow(RestFlowContext context, RestFlowStage startStage)
    {
        RestFlowStage currentStage = startStage;
        while (currentStage != RestFlowStage.Completed)
        {
            switch (currentStage)
            {
                case RestFlowStage.DayEnd:
                    context.CurrentStage = RestFlowStage.DayEnd;
                    if (ExecuteDayEndHandlers(context))
                    {
                        return RestBeginResult.Rejected;
                    }
                    if (context.IsStopped)
                    {
                        return RestBeginResult.Rejected;
                    }

                    if (PauseFlowForStory(context, RestStoryPhase.DayEnd, RestFlowStage.RestResolve))
                    {
                        return RestBeginResult.Paused;
                    }

                    currentStage = RestFlowStage.RestResolve;
                    break;
                case RestFlowStage.RestResolve:
                    context.CurrentStage = RestFlowStage.RestResolve;
                    ExecuteSettlementHandlers(context);
                    currentStage = RestFlowStage.WaitForPlayerConfirm;
                    break;
                case RestFlowStage.WaitForPlayerConfirm:
                    context.CurrentStage = RestFlowStage.WaitForPlayerConfirm;
                    EnterWaitForPlayerConfirm(context);
                    return RestBeginResult.Started;
                case RestFlowStage.DayStart:
                    context.CurrentStage = RestFlowStage.DayStart;
                    if (ExecuteDayStartHandlers(context))
                    {
                        return RestBeginResult.Rejected;
                    }

                    if (context.IsStopped)
                    {
                        return RestBeginResult.Rejected;
                    }

                    if (PauseFlowForStory(context, RestStoryPhase.DayStart, RestFlowStage.Finalize))
                    {
                        return RestBeginResult.Paused;
                    }

                    currentStage = RestFlowStage.Finalize;
                    break;
                case RestFlowStage.Finalize:
                    context.CurrentStage = RestFlowStage.Finalize;
                    FinalizeFlow(context);
                    currentStage = RestFlowStage.Completed;
                    break;
                default:
                    currentStage = RestFlowStage.Completed;
                    break;
            }
        }

        context.CurrentStage = RestFlowStage.Completed;
        return RestBeginResult.Started;
    }

    private bool ExecuteDayEndHandlers(RestFlowContext context)
    {
        for (int i = 0; i < _dayEndHandlers.Count; i++)
        {
            RestFlowDirective directive = _dayEndHandlers[i].Execute(context);
            if (directive == RestFlowDirective.Pause)
            {
                Debug.LogWarning("RestManager.ExecuteDayEndHandlers -> 已禁用 handler 级 Pause，请改为走 RestStoryBusGateway。");
                continue;
            }

            if (directive == RestFlowDirective.Stop)
            {
                context.IsStopped = true;
                return false;
            }
        }

        return false;
    }

    private void ExecuteSettlementHandlers(RestFlowContext context)
    {
        for (int i = 0; i < _restSettlementHandlers.Count; i++)
        {
            _restSettlementHandlers[i].Execute(context);
        }
    }

    private bool ExecuteDayStartHandlers(RestFlowContext context)
    {
        for (int i = 0; i < _dayStartHandlers.Count; i++)
        {
            RestFlowDirective directive = _dayStartHandlers[i].Execute(context);
            if (directive == RestFlowDirective.Pause)
            {
                Debug.LogWarning("RestManager.ExecuteDayStartHandlers -> 已禁用 handler 级 Pause，请改为走 RestStoryBusGateway。");
                continue;
            }

            if (directive == RestFlowDirective.Stop)
            {
                context.IsStopped = true;
                return false;
            }
        }

        return false;
    }

    private bool PauseFlowForStory(RestFlowContext context, RestStoryPhase phase, RestFlowStage resumeStage)
    {
        if (!_storyBusGateway.TryPauseForStory(phase, context, CompleteStoryPausedFlow))
        {
            return false;
        }

        context.IsPaused = true;
        context.ResumeStage = resumeStage;
        _storyPausedFlowContext = context;
        return true;
    }

    private void CompleteStoryPausedFlow()
    {
        if (_storyPausedFlowContext == null)
        {
            return;
        }

        RestFlowContext context = _storyPausedFlowContext;
        _storyPausedFlowContext = null;
        context.IsPaused = false;
        AdvanceFlow(context, context.ResumeStage);
    }

    private void EnterWaitForPlayerConfirm(RestFlowContext context)
    {
        if (context == null)
        {
            return;
        }

        _waitForPlayerConfirmContext = context;
        UIManager.Instance.ShowRestSummary(context.Settlement);
    }

    private void FinalizeFlow(RestFlowContext context)
    {
        if (context == null)
        {
            return;
        }

        UIManager.Instance.UpdateInfo();
        OnRestSettled?.Invoke(context.Settlement);
    }

    private static void RegisterHandler<T>(List<T> handlers, T handler)
    {
        if (handlers == null || handler == null || handlers.Contains(handler))
        {
            return;
        }

        handlers.Add(handler);
    }
}

