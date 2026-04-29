using UnityEngine;

[CreateAssetMenu(fileName = "NewConditionPredicate", menuName = "GoHome/Event/ConditionPredicate")]
public abstract class EventConditionPredicate : ScriptableObject
{
    // 预留接口：后续可扩展道具、状态、天气、任务阶段等判定
    public abstract bool IsMet(EventConditionContext context);
}

public struct EventConditionContext
{
    public int Distance;
    public int Day;
    public int RegionId;
}
