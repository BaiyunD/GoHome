using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTraitCommon", menuName = "GoHome/Enemy/EnemyTrait/Common")]
public sealed class EnemyBattleTraitCommonAsset : EnemyBattleTraitAsset
{
    [SerializeField] private List<EnemyTraitEffectLine> effectLines = new List<EnemyTraitEffectLine>();

    [TextArea(1, 3)]
    [SerializeField] private string flavorClause = string.Empty;

    public IReadOnlyList<EnemyTraitEffectLine> EffectLines => effectLines;
    public string FlavorClause => flavorClause ?? string.Empty;

    public override bool TryExecuteAndCompose(ref EnemyTraitExecutionContext context, out string bracketBlock)
    {
        bracketBlock = null;
        var genericClauses = new List<string>();
        IReadOnlyList<EnemyTraitEffectLine> lines = effectLines;
        if (lines != null)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                EnemyTraitEffectLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                if (line.Kind == EnemyTraitEffectKind.None
                    && string.IsNullOrWhiteSpace(line.CustomNarrationFragment))
                {
                    continue;
                }

                if (!EnemyBattleTraitRunner.TryApplyEffectLine(line, context.Player, context.Enemy))
                {
                    continue;
                }

                if (EnemyTraitNarrationComposer.TryFormatEffectLine(line, context.EnemyBaseName, out string clause)
                    && !string.IsNullOrEmpty(clause))
                {
                    genericClauses.Add(clause);
                }
            }
        }

        bracketBlock = EnemyTraitNarrationComposer.ComposeFullBlock(
            context.EnemyBaseName,
            TraitDisplayName,
            genericClauses,
            flavorClause ?? string.Empty);
        return !string.IsNullOrEmpty(bracketBlock);
    }
}
