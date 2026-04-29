/// <summary>
/// 特性效果挂载时的运行时上下文。
/// </summary>
public sealed class TraitRuntimeContext
{
    public TraitRuntimeContext(TraitOwner owner, string traitId, TraitManager manager)
    {
        Owner = owner;
        TraitId = traitId;
        Manager = manager;
    }

    public TraitOwner Owner { get; }
    public string TraitId { get; }
    public TraitManager Manager { get; }
}
