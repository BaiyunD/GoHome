using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBattleTrait", menuName = "GoHome/Combat/Enemy Battle Trait")]
public sealed class EnemyBattleTraitAsset : ScriptableObject
{
    [SerializeField] private string traitDisplayName = "特性";

    [SerializeField] private EnemyBattleTraitTrigger trigger = EnemyBattleTraitTrigger.OnEnemyReceiveHit;

    [Tooltip("勾选后仅当本下对敌实际扣血大于 0 时触发（默认关闭，0 伤仍触发）。")]
    [SerializeField] private bool onlyWhenEnemyLostHpFromPlayerHit;

    [Tooltip("勾选后仅当本下对玩家实际扣血大于 0 时触发（用于 OnEnemyAttackEnd）。")]
    [SerializeField] private bool onlyWhenPlayerLostHpFromEnemyHit;

    [SerializeField] private List<EnemyTraitEffectLine> effectLines = new List<EnemyTraitEffectLine>();

    [TextArea(1, 3)]
    [SerializeField] private string specialClause = string.Empty;

    [SerializeField] private EnemyTraitSpecialClauseSlot specialClauseSlot = EnemyTraitSpecialClauseSlot.None;

    [TextArea(1, 3)]
    [SerializeField] private string flavorClause = string.Empty;

    public string TraitDisplayName => traitDisplayName ?? string.Empty;
    public EnemyBattleTraitTrigger Trigger => trigger;
    public bool OnlyWhenEnemyLostHpFromPlayerHit => onlyWhenEnemyLostHpFromPlayerHit;
    public bool OnlyWhenPlayerLostHpFromEnemyHit => onlyWhenPlayerLostHpFromEnemyHit;
    public IReadOnlyList<EnemyTraitEffectLine> EffectLines => effectLines;
    public string SpecialClause => specialClause ?? string.Empty;
    public EnemyTraitSpecialClauseSlot SpecialClauseSlot => specialClauseSlot;
    public string FlavorClause => flavorClause ?? string.Empty;
}
