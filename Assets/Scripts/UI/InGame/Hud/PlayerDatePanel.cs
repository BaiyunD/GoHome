using System.Text;
using TMPro;
using UnityEngine;

public class PlayerDatePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text playerStatusText;

    private void Start()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.PlayerRuntimeChanged += OnPlayerRuntimeChanged;
        }
    }

    private void OnEnable()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.PlayerRuntimeChanged -= OnPlayerRuntimeChanged;
            PlayerStateManager.Instance.PlayerRuntimeChanged += OnPlayerRuntimeChanged;
        }

        if (SurvivalResourceManager.Instance != null)
        {
            SurvivalResourceManager.Instance.ResourceChanged -= OnSurvivalResourceChanged;
            SurvivalResourceManager.Instance.ResourceChanged += OnSurvivalResourceChanged;
        }

        UpdateInfo();
    }

    private void OnDestroy()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.PlayerRuntimeChanged -= OnPlayerRuntimeChanged;
        }

        if (SurvivalResourceManager.Instance != null)
        {
            SurvivalResourceManager.Instance.ResourceChanged -= OnSurvivalResourceChanged;
        }
    }

    private void OnPlayerRuntimeChanged(PlayerRuntime runtime)
    {
        UpdateInfo();
    }

    private void OnDisable()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.PlayerRuntimeChanged -= OnPlayerRuntimeChanged;
        }

        if (SurvivalResourceManager.Instance != null)
        {
            SurvivalResourceManager.Instance.ResourceChanged -= OnSurvivalResourceChanged;
        }
    }

    public void UpdateInfo()
    {
        RefreshStatusText();
    }

    public void UpdateHP()
    {
        RefreshStatusText();
    }

    public void UpdateHunger()
    {
        RefreshStatusText();
    }

    public void UpdateHealth()
    {
        RefreshStatusText();
    }

    public void UpdateEnergy()
    {
        RefreshStatusText();
    }

    public void UpdateMoney()
    {
        RefreshStatusText();
    }

    private void OnSurvivalResourceChanged(SurvivalResourceType type)
    {
        RefreshStatusText();
    }

    private void RefreshStatusText()
    {
        if (playerStatusText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        PlayerRuntime runtime = PlayerStateManager.Instance != null ? PlayerStateManager.Instance.Current : null;
        string hpText = FormatHpValue(runtime);
        if (BattleManager.Instance != null)
        {
            BattleManager battleManager = BattleManager.Instance;
            bool inBattle = battleManager.Phase != BattlePhase.None && battleManager.Phase != BattlePhase.Ended;
            if (inBattle && battleManager.PlayerRuntime != null)
            {
                hpText = FormatHpValue(battleManager.PlayerRuntime);
            }
        }
        builder.Append("HP：").Append(hpText).Append('\n');
        builder.Append("精力：").Append(FormatSurvivalValue(SurvivalResourceType.Energy)).Append('\n');
        builder.Append("饱食度：").Append(FormatSurvivalValue(SurvivalResourceType.Hunger)).Append('\n');
        builder.Append("健康：").Append(FormatSurvivalCurrentOnly(SurvivalResourceType.Health)).Append('\n');
        builder.Append("金钱：").Append(FormatMoneyValue()).Append("元");
        playerStatusText.text = builder.ToString();
    }

    private static string FormatHpValue(CharacterRuntimeStats runtime)
    {
        if (runtime == null)
        {
            return "null";
        }

        return $"{runtime.CurrentHp}/{runtime.MaxHp}";
    }

    private static string FormatHpValue(PlayerRuntime runtime)
    {
        if (runtime == null)
        {
            return "null";
        }

        return $"{Mathf.RoundToInt(runtime.CurrentHp)}/{Mathf.RoundToInt(runtime.MaxHp)}";
    }

    private static string FormatSurvivalCurrentOnly(SurvivalResourceType type)
    {
        if (SurvivalResourceManager.Instance == null)
        {
            return "null";
        }

        bool hasCurrent = SurvivalResourceManager.Instance.TryGetValue(type, out float current);
        if (!hasCurrent)
        {
            return "null";
        }

        return Mathf.RoundToInt(current).ToString();
    }

    private static string FormatSurvivalValue(SurvivalResourceType type)
    {
        if (SurvivalResourceManager.Instance == null)
        {
            return "null";
        }

        bool hasCurrent = SurvivalResourceManager.Instance.TryGetValue(type, out float current);
        bool hasMax = SurvivalResourceManager.Instance.TryGetMaxValue(type, out float max);
        if (!hasCurrent || !hasMax)
        {
            return "null";
        }

        return $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
    }

    private static string FormatMoneyValue()
    {
        if (PlayerResourceService.Instance == null)
        {
            return "null";
        }

        if (!PlayerResourceService.Instance.TryGetValue(PlayerResourceType.Money, out float money))
        {
            return "null";
        }

        return money.ToString("0.00");
    }

    public void Show()
    {
        UpdateInfo();
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

