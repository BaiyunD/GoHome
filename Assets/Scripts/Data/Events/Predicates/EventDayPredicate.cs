using UnityEngine;

[CreateAssetMenu(fileName = "EventDayPredicate", menuName = "GoHome/Event/Predicates/Day")]
public class EventDayPredicate : EventConditionPredicate
{
    public int minDay = -1;
    public int maxDay = -1;

    public override bool IsMet(EventConditionContext context)
    {
        if (minDay != -1 && context.Day < minDay) return false;
        if (maxDay != -1 && context.Day > maxDay) return false;
        return true;
    }
}
