using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpitFruitBattleTrait", menuName = "GoHome/Enemy/EnemyTrait/Special/Enemy Spit Fruit")]
public sealed class EnemySpitFruitBattleTrait : EnemyBattleTraitAsset
{
    [SerializeField] private int itemId = 105;

    [SerializeField] private int grantCount = 1;

    [SerializeField] private string narrationFlavor = "小兔子向你吐了一个果子";

    public override bool TryExecuteAndCompose(ref EnemyTraitExecutionContext context, out string bracketBlock)
    {
        bracketBlock = null;
        if (context.Hook != EnemyBattleTraitHookPhase.OnEnemyAttackEnd)
        {
            return false;
        }

        if (itemId <= 0 || grantCount <= 0)
        {
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("EnemySpitFruitBattleTrait: InventoryManager.Instance is null; cannot grant item.");
            return false;
        }

        InventoryManager.Instance.AddItem(itemId, grantCount);

        string flavor = narrationFlavor != null ? narrationFlavor.Trim() : string.Empty;
        if (string.IsNullOrEmpty(flavor))
        {
            return false;
        }

        bracketBlock = EnemyTraitNarrationComposer.ComposeFullBlock(
            context.EnemyBaseName,
            TraitDisplayName,
            new List<string>(),
            flavor);
        return !string.IsNullOrEmpty(bracketBlock);
    }
}
