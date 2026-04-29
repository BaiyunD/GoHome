/// <summary>
/// 特性效果基类：每个特性效果单独实现，由 TraitManager 在添加/移除时 Attach/Detach。
/// </summary>
public abstract class TraitEffectBase
{
    protected TraitRuntimeContext Context { get; private set; }

    public void Attach(TraitRuntimeContext context)
    {
        Context = context;
        OnAttach();
    }

    public void Detach()
    {
        OnDetach();
        Context = null;
    }

    /// <summary>订阅事件、初始化状态。</summary>
    protected virtual void OnAttach()
    {
    }

    /// <summary>退订事件、清理状态。</summary>
    protected virtual void OnDetach()
    {
    }
}
