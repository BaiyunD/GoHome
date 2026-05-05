using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPoisonStackBattleTrait", menuName = "GoHome/Enemy/EnemyTrait/Special/Enemy Poison Stack")]
public sealed class EnemyPoisonStackBattleTrait : EnemyBattleTraitAsset
{
    [Tooltip("叠层基数 x：伤害 = 当前层数 × max(本场已登记的 x)；每次敌攻结束触发成功时层数 +1。")]
    [SerializeField] private int poisonBasePerLayer = 1;

    public int PoisonBasePerLayer => poisonBasePerLayer;

    public override bool TryExecuteAndCompose(ref EnemyTraitExecutionContext context, out string bracketBlock)
    {
        bracketBlock = null;
        if (context.Hook != EnemyBattleTraitHookPhase.OnEnemyAttackEnd)
        {
            return false;
        }

        int x = poisonBasePerLayer;
        if (x <= 0 || BattleManager.Instance == null)
        {
            return false;
        }

        if (!BattleManager.Instance.TryApplyEnemyPoisonStackFromTrait(x, out int damage, out int stacks))
        {
            return false;
        }

        string clause = EnemyPoisonTraitNarration.FormatPoisonRoundEndClause(damage, stacks);
        if (string.IsNullOrEmpty(clause))
        {
            return false;
        }

        var clauses = new List<string> { clause };
        bracketBlock = EnemyTraitNarrationComposer.ComposeFullBlock(
            context.EnemyBaseName,
            TraitDisplayName,
            clauses,
            string.Empty);
        return !string.IsNullOrEmpty(bracketBlock);
    }
}
