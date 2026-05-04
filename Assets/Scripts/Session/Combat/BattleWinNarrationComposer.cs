using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class BattleWinNarrationComposer
{
    public static string Compose(
        string baseStatClause,
        IReadOnlyList<string> commonAcquireParts,
        IReadOnlyList<string> extraAcquireParts,
        string extraVictoryDescription)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("战斗胜利！");
        if (!string.IsNullOrWhiteSpace(baseStatClause))
        {
            sb.Append(baseStatClause);
            sb.Append("！");
        }

        List<string> allAcquire = new List<string>();
        if (commonAcquireParts != null)
        {
            for (int i = 0; i < commonAcquireParts.Count; i++)
            {
                string p = commonAcquireParts[i];
                if (!string.IsNullOrWhiteSpace(p))
                {
                    allAcquire.Add(p);
                }
            }
        }

        if (extraAcquireParts != null)
        {
            for (int i = 0; i < extraAcquireParts.Count; i++)
            {
                string p = extraAcquireParts[i];
                if (!string.IsNullOrWhiteSpace(p))
                {
                    allAcquire.Add(p);
                }
            }
        }

        if (allAcquire.Count > 0)
        {
            sb.Append("获得");
            for (int i = 0; i < allAcquire.Count; i++)
            {
                sb.Append(allAcquire[i]);
            }

            sb.Append("。");
        }

        sb.Append(FormatExtraVictoryDescriptionSuffix(extraVictoryDescription));

        return sb.ToString();
    }

    /// <summary>敌人「胜利补充句」：已带【】则原样返回，否则外包一层【】；空则返回空串。</summary>
    public static string FormatExtraVictoryDescriptionSuffix(string extraVictoryDescription)
    {
        if (string.IsNullOrWhiteSpace(extraVictoryDescription))
        {
            return string.Empty;
        }

        string extra = extraVictoryDescription.Trim();
        if (extra.Length >= 2 && extra[0] == '【' && extra[extra.Length - 1] == '】')
        {
            return extra;
        }

        return $"【{extra}】";
    }

    public static string FormatMoneyAcquirePart(float yuan)
    {
        string y = MoneyUtil.CentsToYuan(MoneyUtil.YuanToCents(yuan)).ToString("0.#");
        return $"【金钱】*{y}";
    }

    public static string FormatItemAcquirePart(string displayName, int count)
    {
        string name = string.IsNullOrWhiteSpace(displayName) ? "物品" : displayName.Trim();
        return $"【{name}】*{count}";
    }

    public static string TryResolveItemDisplayName(int itemId)
    {
        if (itemId <= 0)
        {
            return $"Item({itemId})";
        }

        if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGet(itemId, out ItemBase item) && item != null)
        {
            return item.DisplayName;
        }

        if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGetDefinition(itemId, out ItemDefinition def) && def != null)
        {
            return string.IsNullOrWhiteSpace(def.DisplayName) ? def.name : def.DisplayName;
        }

        return $"Item({itemId})";
    }
}
