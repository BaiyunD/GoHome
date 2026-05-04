using System.Collections.Generic;
using UnityEngine;

public static class BattleWinSettlement
{
    public static string ApplyAndComposeNarration(
        BattleSettlementRewardSnapshot rewards,
        int baseVictoryAttackDelta,
        int baseVictoryDefenseDelta,
        int baseVictoryMaxHpDelta)
    {
        PlayerRuntime player = PlayerStateManager.Instance != null ? PlayerStateManager.Instance.Current : null;
        if (player == null)
        {
            return "战斗胜利。";
        }

        int roll = Random.Range(0, 3);
        string baseClause;
        if (roll == 0)
        {
            player.Attack += Mathf.Max(0, baseVictoryAttackDelta);
            baseClause = $"攻击提高{Mathf.Max(0, baseVictoryAttackDelta)}点";
        }
        else if (roll == 1)
        {
            player.Defense += Mathf.Max(0, baseVictoryDefenseDelta);
            baseClause = $"防御提高{Mathf.Max(0, baseVictoryDefenseDelta)}点";
        }
        else
        {
            int d = Mathf.Max(0, baseVictoryMaxHpDelta);
            player.MaxHp += d;
            player.CurrentHp = Mathf.Min(player.CurrentHp, player.MaxHp);
            baseClause = $"最大生命值提高{d}点";
        }

        List<string> commonParts = new List<string>();
        ApplyEntries(rewards != null ? rewards.Common : null, commonParts);

        List<string> extraParts = new List<string>();
        ApplyEntries(rewards != null ? rewards.Extra : null, extraParts);

        string desc = rewards != null ? rewards.ExtraVictoryDescription : string.Empty;
        return BattleWinNarrationComposer.Compose(baseClause, commonParts, extraParts, desc);
    }

    private static void ApplyEntries(IReadOnlyList<BattleRewardEntry> entries, List<string> acquireParts)
    {
        if (entries == null || acquireParts == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            BattleRewardEntry entry = entries[i];
            if (entry == null || entry.kind == BattleRewardEntry.RewardKind.None)
            {
                continue;
            }

            if (entry.kind == BattleRewardEntry.RewardKind.Item)
            {
                int id = entry.itemId;
                int count = Mathf.Max(0, entry.itemCount);
                if (id <= 0 || count <= 0)
                {
                    continue;
                }

                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(id, count);
                }

                string display = BattleWinNarrationComposer.TryResolveItemDisplayName(id);
                acquireParts.Add(BattleWinNarrationComposer.FormatItemAcquirePart(display, count));
            }
            else if (entry.kind == BattleRewardEntry.RewardKind.Money)
            {
                float yuan = Mathf.Max(0f, entry.moneyYuan);
                if (yuan <= 0f)
                {
                    continue;
                }

                int cents = MoneyUtil.YuanToCents(yuan);
                if (PlayerStateManager.Instance != null && PlayerStateManager.Instance.Current != null)
                {
                    PlayerRuntime p = PlayerStateManager.Instance.Current;
                    p.MoneyCents = MoneyUtil.ClampNonNegativeCents(p.MoneyCents + cents);
                }

                acquireParts.Add(BattleWinNarrationComposer.FormatMoneyAcquirePart(yuan));
            }
        }
    }
}
