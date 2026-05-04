using System.Collections.Generic;

using UnityEngine;



/// <summary>胜利结算：基础三选一属性与奖励条目应用到 <see cref="PlayerStateManager.Current"/>。</summary>

public static class BattleWinSettlement

{

#if UNITY_EDITOR

    /// <summary>为 true 时在 Console 输出基础奖励是否写入 PlayerRuntime（默认关，排查时改为 true）。</summary>

    public static bool LogBaseVictoryRewardApply = false;

#endif



    public static string ApplyAndComposeNarration(

        BattleSettlementRewardSnapshot rewards,

        int baseVictoryAttackDelta,

        int baseVictoryDefenseDelta,

        int baseVictoryMaxHpDelta)

    {

        PlayerRuntime player = PlayerStateManager.Instance != null ? PlayerStateManager.Instance.Current : null;

        if (player == null)

        {

#if UNITY_EDITOR

            if (LogBaseVictoryRewardApply)

            {

                Debug.LogWarning("[BattleWinSettlement] PlayerStateManager.Current 为空，未应用基础奖励。");

            }

#endif

            return "战斗胜利。";

        }



        string baseClause = RollAndApplyBaseVictoryReward(

            player,

            baseVictoryAttackDelta,

            baseVictoryDefenseDelta,

            baseVictoryMaxHpDelta,

            out int loggedRoll);

#if UNITY_EDITOR

        if (LogBaseVictoryRewardApply)

        {

            Debug.Log(

                $"[BattleWinSettlement] 基础奖励已写入 PlayerRuntime roll={loggedRoll} ({baseClause}) -> " +

                $"Attack={player.Attack} Defense={player.Defense} MaxHp={player.MaxHp} CurrentHp={player.CurrentHp}");

        }

#endif



        List<string> commonParts = new List<string>();

        ApplyEntries(rewards != null ? rewards.Common : null, commonParts);



        // extraRewards：仅发放，不写入 EventNarrationModal 的「获得…」与 common 同列展示

        ApplyEntries(rewards != null ? rewards.Extra : null, null);

        // 敌人胜利补充句改由 BattleManager 在幸运石等追加块之后拼接，保证顺序：主结算 → 幸运石 → 补充句
        return BattleWinNarrationComposer.Compose(baseClause, commonParts, null, string.Empty);

    }



    /// <summary>
    /// 幸运石等追加结算：再一次随机基础三选一 + 仅 commonRewards（叙述与发放）；不发放 extra。
    /// 敌人「胜利补充句」由 <see cref="BattleManager"/> 在幸运石块之后统一拼接。
    /// </summary>
    public static string ApplyBonusBaseAndCommonOnly(
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

        string baseClause = RollAndApplyBaseVictoryReward(
            player,
            baseVictoryAttackDelta,
            baseVictoryDefenseDelta,
            baseVictoryMaxHpDelta,
            out _);

        List<string> commonParts = new List<string>();
        ApplyEntries(rewards != null ? rewards.Common : null, commonParts);

        return BattleWinNarrationComposer.Compose(baseClause, commonParts, null, string.Empty);
    }



    /// <summary>随机基础三选一并写入 <see cref="PlayerRuntime.CombatBase"/>，返回属性 clause（不含「战斗胜利！」前缀）。</summary>

    private static string RollAndApplyBaseVictoryReward(

        PlayerRuntime player,

        int baseVictoryAttackDelta,

        int baseVictoryDefenseDelta,

        int baseVictoryMaxHpDelta,

        out int roll)

    {

        roll = Random.Range(0, 3);

        if (roll == 0)

        {

            player.CombatBase.Attack += Mathf.Max(0, baseVictoryAttackDelta);

            player.RefreshFlattenedCombatFromLayers();

            return $"攻击提高{Mathf.Max(0, baseVictoryAttackDelta)}点";

        }



        if (roll == 1)

        {

            player.CombatBase.Defense += Mathf.Max(0, baseVictoryDefenseDelta);

            player.RefreshFlattenedCombatFromLayers();

            return $"防御提高{Mathf.Max(0, baseVictoryDefenseDelta)}点";

        }



        int d = Mathf.Max(0, baseVictoryMaxHpDelta);

        player.CombatBase.MaxHp += d;

        player.RefreshFlattenedCombatFromLayers();

        player.CurrentHp = Mathf.Min(player.CurrentHp, player.MaxHp);

        return $"最大生命值提高{d}点";

    }



    /// <param name="acquireParts">为 null 时只发放物品/金钱，不生成叙述片段。</param>

    private static void ApplyEntries(IReadOnlyList<BattleRewardEntry> entries, List<string> acquireParts)

    {

        if (entries == null)

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



                if (acquireParts != null)

                {

                    string display = BattleWinNarrationComposer.TryResolveItemDisplayName(id);

                    acquireParts.Add(BattleWinNarrationComposer.FormatItemAcquirePart(display, count));

                }

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



                if (acquireParts != null)

                {

                    acquireParts.Add(BattleWinNarrationComposer.FormatMoneyAcquirePart(yuan));

                }

            }

        }

    }

}


