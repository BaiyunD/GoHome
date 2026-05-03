/// <summary>特殊句相对「通用效果句」的插入位置（风味句始终在括号内最后一段）。</summary>
public enum EnemyTraitSpecialClauseSlot
{
    None = 0,
    BeforeGeneric = 1,
    AfterGeneric = 2,

    /// <summary>与 <see cref="AfterGeneric"/> 相同语义：通用句之后、风味句之前。</summary>
    End = 3
}
