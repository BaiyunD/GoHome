using UnityEngine;

/// <summary>
/// 敌人战斗特性数据基类（无 Create 菜单）。通用模版见 <see cref="EnemyBattleTraitCommonAsset"/>；
/// 特殊机制为直接继承本类的叶子类型（例如 <see cref="EnemyPoisonStackBattleTrait"/>）。
/// </summary>
public class EnemyBattleTraitAsset : ScriptableObject
{
    [SerializeField] private string traitDisplayName = "特性";

    [SerializeField] private EnemyBattleTraitTrigger trigger = EnemyBattleTraitTrigger.OnEnemyReceiveHit;

    [Tooltip("勾选后仅当本下对敌实际扣血大于 0 时触发（默认关闭，0 伤仍触发）。")]
    [SerializeField] private bool onlyWhenEnemyLostHpFromPlayerHit;

    [Tooltip("勾选后仅当本下对玩家实际扣血大于 0 时触发（用于 OnEnemyAttackEnd）。")]
    [SerializeField] private bool onlyWhenPlayerLostHpFromEnemyHit;

    public string TraitDisplayName => traitDisplayName ?? string.Empty;
    public EnemyBattleTraitTrigger Trigger => trigger;
    public bool OnlyWhenEnemyLostHpFromPlayerHit => onlyWhenEnemyLostHpFromPlayerHit;
    public bool OnlyWhenPlayerLostHpFromEnemyHit => onlyWhenPlayerLostHpFromEnemyHit;

    public virtual bool TryExecuteAndCompose(ref EnemyTraitExecutionContext context, out string bracketBlock)
    {
        bracketBlock = null;
        return false;
    }
}
