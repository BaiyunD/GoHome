using UnityEngine;

[CreateAssetMenu(fileName = "EventDistancePredicate", menuName = "GoHome/Event/Predicates/Distance")]
public class EventDistancePredicate : EventConditionPredicate
{
    public int minDistance = -1;
    public int maxDistance = -1;

    public override bool IsMet(EventConditionContext context)
    {
        if (minDistance != -1 && context.Distance < minDistance) return false;
        if (maxDistance != -1 && context.Distance > maxDistance) return false;
        return true;
    }
}
