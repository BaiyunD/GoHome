/// <summary>
/// 敌人战斗特性单条效果类型；数值含义见 <see cref="EnemyTraitEffectLine"/> 注释。
/// </summary>
public enum EnemyTraitEffectKind
{
    None = 0,

    /// <summary>对玩家造成 intValue 点额外伤害（与普攻伤害独立结算）。</summary>
    PlayerFlatDamage = 1,

    /// <summary>玩家攻击 ±intValue（负数为下降）。</summary>
    PlayerAttackDelta = 2,

    /// <summary>玩家防御 ±intValue。</summary>
    PlayerDefenseDelta = 3,

    /// <summary>玩家暴击率 ±floatValue（百分点，如 -5 表示 -5%）。</summary>
    PlayerCriticalRateDelta = 4,

    /// <summary>玩家暴击伤害 ±floatValue（与角色面板一致，为最终倍率增减）。</summary>
    PlayerCriticalDamageDelta = 5,

    /// <summary>玩家格挡率 ±floatValue（百分点）。</summary>
    PlayerBlockRateDelta = 6,

    /// <summary>玩家闪避率 ±floatValue（百分点）。</summary>
    PlayerDodgeRateDelta = 7,

    /// <summary>玩家逃跑率 ±floatValue（百分点）。</summary>
    PlayerEscapeRateDelta = 8,

    /// <summary>敌人恢复 intValue 生命。</summary>
    EnemyHeal = 20,

    /// <summary>敌人攻击 ±intValue。</summary>
    EnemyAttackDelta = 21,

    /// <summary>敌人防御 ±intValue。</summary>
    EnemyDefenseDelta = 22,

    /// <summary>敌人暴击率 ±floatValue（百分点）。</summary>
    EnemyCriticalRateDelta = 23,

    /// <summary>敌人暴击伤害 ±floatValue。</summary>
    EnemyCriticalDamageDelta = 24,

    /// <summary>敌人格挡率 ±floatValue（百分点）。</summary>
    EnemyBlockRateDelta = 25,

    /// <summary>敌人闪避率 ±floatValue（百分点）。</summary>
    EnemyDodgeRateDelta = 26
}
