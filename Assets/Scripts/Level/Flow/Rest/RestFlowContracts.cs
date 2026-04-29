using System.Collections.Generic;

public enum RestFlowStage
{
    None = 0,
    Precheck = 1,
    DayEnd = 2,
    RestResolve = 3,
    WaitForPlayerConfirm = 4,
    DayStart = 5,
    Finalize = 6,
    Completed = 7
}

public enum RestFlowDirective
{
    Continue = 0,
    Pause = 1,
    Stop = 2
}

public enum RestBeginResult
{
    Rejected = 0,
    NeedSecondClick = 1,
    Started = 2,
    Paused = 3
}

public enum RestPrecheckDecision
{
    Pass = 0,
    NeedSecondClick = 1,
    Reject = 2
}

public sealed class RestPrecheckResult
{
    public static readonly RestPrecheckResult Pass = new RestPrecheckResult(RestPrecheckDecision.Pass);

    public RestPrecheckDecision Decision
    {
        get;
    }

    public string Message
    {
        get;
    }

    public RestPrecheckResult(RestPrecheckDecision decision, string message = "")
    {
        Decision = decision;
        Message = message ?? string.Empty;
    }
}

public sealed class RestFlowContext
{
    public GameManager GameManager
    {
        get;
    }

    public RestSettlement Settlement
    {
        get;
    }

    public RestContext RestContext
    {
        get;
        set;
    }

    public RestFlowStage CurrentStage
    {
        get;
        set;
    }

    public RestFlowStage ResumeStage
    {
        get;
        set;
    }

    public bool HasConfirmedEnergySurplus
    {
        get;
    }

    public bool IsPaused
    {
        get;
        set;
    }

    public bool IsStopped
    {
        get;
        set;
    }

    public string BlockMessage
    {
        get;
        set;
    }

    public List<string> DayEndMessages
    {
        get;
    } = new List<string>();

    public List<string> DayStartMessages
    {
        get;
    } = new List<string>();

    public RestFlowContext(GameManager gameManager, bool hasConfirmedEnergySurplus)
    {
        GameManager = gameManager;
        Settlement = new RestSettlement();
        HasConfirmedEnergySurplus = hasConfirmedEnergySurplus;
        CurrentStage = RestFlowStage.None;
        ResumeStage = RestFlowStage.None;
        BlockMessage = string.Empty;
    }
}

public interface IRestPrecheckHandler
{
    RestPrecheckResult Evaluate(RestFlowContext context);
}

public interface IDayEndHandler
{
    RestFlowDirective Execute(RestFlowContext context);
}

public interface IRestSettlementHandler
{
    void Execute(RestFlowContext context);
}

public interface IDayStartHandler
{
    RestFlowDirective Execute(RestFlowContext context);
}
