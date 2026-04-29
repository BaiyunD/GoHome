using System;
using System.Text;
using TMPro;
using UnityEngine;

public class CombatStatsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text combatStatsText;

    private readonly (string Label, Func<CharacterRuntimeStats, float> Getter, bool IsPercent)[] _combatStats =
    {
        ("HP", runtime => runtime.CurrentHp, false),
        ("攻击", runtime => runtime.Attack, false),
        ("防御", runtime => runtime.Defense, false),
        ("暴击率", runtime => runtime.CriticalRate, true),
        ("暴击伤害", runtime => runtime.CriticalDamage, true),
        ("格挡率", runtime => runtime.BlockRate, true),
        ("闪避率", runtime => runtime.DodgeRate, true)
    };

    private void OnEnable()
    {
        RefreshCombatStats();
    }

    public void OnOpenCombatStatsPage()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.OpenUIEntry(UIKey.CombatStats);
    }

    public void OnOpenFromHUD()
    {
        // Legacy transition entry. Prefer OnOpenCombatStatsPage().
        OnOpenCombatStatsPage();
    }

    public void OnCloseCombatStatsPage()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.CloseUIEntry(UIKey.CombatStats);
    }

    public void OnClose()
    {
        // Legacy transition entry. Prefer OnCloseCombatStatsPage().
        OnCloseCombatStatsPage();
    }

    public void Show()
    {
        RefreshCombatStats();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshCombatStats()
    {
        if (combatStatsText == null)
        {
            return;
        }

        CharacterRuntimeStats runtime = ResolveRuntimeStats();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < _combatStats.Length; i++)
        {
            string label = _combatStats[i].Label;
            float value = runtime != null ? _combatStats[i].Getter(runtime) : 0f;
            string valueText = label == "HP"
                ? FormatHpValue(runtime)
                : FormatStatValue(value, runtime != null, _combatStats[i].IsPercent);
            if (i > 0)
            {
                builder.Append('\n');
            }
            builder.Append(label).Append('：').Append(valueText);
        }
        combatStatsText.text = builder.ToString();
    }

    private static CharacterRuntimeStats ResolveRuntimeStats()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager battleManager = BattleManager.Instance;
            bool inBattle = battleManager.Phase != BattlePhase.None && battleManager.Phase != BattlePhase.Ended;
            if (inBattle && battleManager.PlayerRuntime != null)
            {
                return battleManager.PlayerRuntime;
            }
        }

        PlayerRuntime playerRuntime = PlayerStateManager.Instance != null ? PlayerStateManager.Instance.Current : null;
        if (playerRuntime == null)
        {
            return null;
        }

        return new CharacterRuntimeStats(playerRuntime, playerRuntime.TraitIds, playerRuntime.EscapeRate);
    }

    private static string FormatHpValue(CharacterRuntimeStats runtime)
    {
        if (runtime == null)
        {
            return "null";
        }

        return $"{runtime.CurrentHp}/{runtime.MaxHp}";
    }
    private static string FormatStatValue(float value, bool hasValue, bool isPercent)
    {
        if (!hasValue)
        {
            return "null";
        }

        float rounded = Mathf.Round(value);
        if (Mathf.Abs(value - rounded) < 0.001f)
        {
            string intText = rounded.ToString("F0");
            return isPercent ? $"{intText}%" : intText;
        }

        string floatText = value.ToString("F1");
        return isPercent ? $"{floatText}%" : floatText;
    }
}

