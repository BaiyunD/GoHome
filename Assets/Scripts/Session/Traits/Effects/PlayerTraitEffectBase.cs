/// <summary>
/// 玩家侧特性效果：按触发时机扩展。
/// </summary>
public abstract class PlayerTraitEffectBase : TraitEffectBase
{
    /// <summary>子类声明本效果关心的触发时机。</summary>
    public abstract PlayerTraitTrigger TriggerKind { get; }

    public virtual void OnExplore()
    {
    }

    public virtual void OnAttack()
    {
    }

    public virtual void OnPermanentStatApply()
    {
    }

    public virtual void OnRestSettlement(RestContext context)
    {
    }

    public virtual void OnDayStart(RestContext context)
    {
    }

    public virtual void OnOtherTrigger()
    {
    }
}
