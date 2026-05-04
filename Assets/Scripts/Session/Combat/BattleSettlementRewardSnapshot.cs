using System.Collections.Generic;

/// <summary>
/// 战斗结束时从 EnemyData 拷贝的奖励快照（避免 ClearCurrent 时 Destroy 运行态资产后无法读取）。
/// </summary>
public sealed class BattleSettlementRewardSnapshot
{
    public BattleSettlementRewardSnapshot(
        IReadOnlyList<BattleRewardEntry> common,
        IReadOnlyList<BattleRewardEntry> extra,
        string extraVictoryDescription)
    {
        Common = CloneEntries(common);
        Extra = CloneEntries(extra);
        ExtraVictoryDescription = extraVictoryDescription ?? string.Empty;
    }

    public List<BattleRewardEntry> Common { get; }

    public List<BattleRewardEntry> Extra { get; }

    public string ExtraVictoryDescription { get; }

    public static BattleSettlementRewardSnapshot Empty()
    {
        return new BattleSettlementRewardSnapshot(
            System.Array.Empty<BattleRewardEntry>(),
            System.Array.Empty<BattleRewardEntry>(),
            string.Empty);
    }

    public static BattleSettlementRewardSnapshot FromEnemyData(EnemyData data)
    {
        if (data == null)
        {
            return Empty();
        }

        return new BattleSettlementRewardSnapshot(
            data.CommonRewards,
            data.ExtraRewards,
            data.ExtraVictoryDescription);
    }

    private static List<BattleRewardEntry> CloneEntries(IReadOnlyList<BattleRewardEntry> source)
    {
        List<BattleRewardEntry> list = new List<BattleRewardEntry>();
        if (source == null)
        {
            return list;
        }

        for (int i = 0; i < source.Count; i++)
        {
            BattleRewardEntry src = source[i];
            if (src == null)
            {
                continue;
            }

            list.Add(new BattleRewardEntry
            {
                kind = src.kind,
                itemId = src.itemId,
                itemCount = src.itemCount,
                moneyYuan = src.moneyYuan
            });
        }

        return list;
    }
}
