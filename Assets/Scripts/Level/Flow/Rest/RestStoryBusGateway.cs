using System;
using UnityEngine;

public enum RestStoryPhase
{
    DayEnd = 0,
    DayStart = 1
}

public sealed class RestStoryBusRequest
{
    public RestStoryPhase Phase
    {
        get;
    }

    public RestFlowContext Context
    {
        get;
    }

    public RestStoryBusRequest(RestStoryPhase phase, RestFlowContext context)
    {
        Phase = phase;
        Context = context;
    }
}

public sealed class RestStoryBusGateway
{
    private Action _pendingCompleteAction;

    public event Action<RestStoryBusRequest> StoryRequested;

    public bool TryPauseForStory(RestStoryPhase phase, RestFlowContext context, Action onComplete)
    {
        if (onComplete == null)
        {
            return false;
        }

        if (StoryRequested == null)
        {
            return false;
        }

        _pendingCompleteAction = onComplete;
        StoryRequested.Invoke(new RestStoryBusRequest(phase, context));
        return true;
    }

    public void CompleteCurrentStory()
    {
        if (_pendingCompleteAction == null)
        {
            Debug.LogWarning("RestStoryBusGateway.CompleteCurrentStory -> 当前没有可恢复的休息剧情流程。");
            return;
        }

        Action action = _pendingCompleteAction;
        _pendingCompleteAction = null;
        action.Invoke();
    }
}
