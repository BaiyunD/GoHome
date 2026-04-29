/// <summary>
/// 特性来源（获得途径）。
/// </summary>
public enum TraitSource
{
    Player,
    Enemy,
    Partner,
    Item,
    Other
}

/// <summary>
/// 特性当前挂载在谁身上（由 TraitManager 管理）。
/// </summary>
public enum TraitOwner
{
    Player,
    Enemy
}

/// <summary>
/// 玩家特性效果触发时机（可随玩法扩展）。
/// </summary>
public enum PlayerTraitTrigger
{
    Explore,
    Attack,
    PermanentStatBonus,
    RestSettlement,
    DayStart,
    Other
}
