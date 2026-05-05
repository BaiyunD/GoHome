using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattlePanel : MonoBehaviour
{
    public static BattlePanel Instance
    {
        get; private set;
    }

    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] public Button combatButton;
    [SerializeField] public Button transformButton;
    [SerializeField] private TMP_Text battleForm;
    [SerializeField] public Button fleeButton;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private TMP_Text fleeRateText;
    [SerializeField] private TMP_Text battleLogText;
    [SerializeField] private GameObject controlsRoot;

    private BattleManager _battleManager;
    private string _lastPlayerAttackLine = string.Empty;
    private string _lastEnemyAttackLine = string.Empty;
    private readonly List<TMP_Text> _battleLogTargets = new List<TMP_Text>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        _battleManager = BattleManager.Instance;
        RebuildBattleLogTargets();
        BindButtons();
        SubscribeBattleEvents();
    }

    private void OnDisable()
    {
        UnsubscribeBattleEvents();
        UnbindButtons();
    }

    public void Show()
    {
        _battleManager = BattleManager.Instance;
        this.gameObject.SetActive(true);
        RebuildBattleLogTargets();
        _lastPlayerAttackLine = string.Empty;
        _lastEnemyAttackLine = string.Empty;
        UpdateBattleLogText();
        BindButtons();
        SubscribeBattleEvents();
        RefreshView();
    }

    public void Hide()
    {
        UnsubscribeBattleEvents();
        UnbindButtons();
        this.gameObject.SetActive(false);
    }

    private void BindButtons()
    {
        if (combatButton != null)
        {
            combatButton.onClick.RemoveListener(OnCombatClicked);
            combatButton.onClick.AddListener(OnCombatClicked);
        }

        if (fleeButton != null)
        {
            fleeButton.onClick.RemoveListener(OnFleeClicked);
            fleeButton.onClick.AddListener(OnFleeClicked);
        }
    }

    private void UnbindButtons()
    {
        if (combatButton != null)
        {
            combatButton.onClick.RemoveListener(OnCombatClicked);
        }

        if (fleeButton != null)
        {
            fleeButton.onClick.RemoveListener(OnFleeClicked);
        }
    }

    private void SubscribeBattleEvents()
    {
        if (_battleManager == null)
        {
            _battleManager = BattleManager.Instance;
            if (_battleManager == null)
            {
                return;
            }
        }

        _battleManager.PlayerAttackResolved -= OnPlayerAttackResolved;
        _battleManager.PlayerAttackResolved += OnPlayerAttackResolved;
        _battleManager.EnemyAttackResolved -= OnEnemyAttackResolved;
        _battleManager.EnemyAttackResolved += OnEnemyAttackResolved;
        _battleManager.BattleLogRaised -= OnBattleLogRaised;
        _battleManager.BattleLogRaised += OnBattleLogRaised;
        _battleManager.TurnSubPhaseChanged -= OnTurnSubPhaseChanged;
        _battleManager.TurnSubPhaseChanged += OnTurnSubPhaseChanged;
        _battleManager.ActionPresentationCompleted -= OnActionPresentationCompleted;
        _battleManager.ActionPresentationCompleted += OnActionPresentationCompleted;
        _battleManager.BattleEnded -= OnBattleEnded;
        _battleManager.BattleEnded += OnBattleEnded;
    }

    private void UnsubscribeBattleEvents()
    {
        if (_battleManager == null)
        {
            return;
        }

        _battleManager.PlayerAttackResolved -= OnPlayerAttackResolved;
        _battleManager.EnemyAttackResolved -= OnEnemyAttackResolved;
        _battleManager.BattleLogRaised -= OnBattleLogRaised;
        _battleManager.TurnSubPhaseChanged -= OnTurnSubPhaseChanged;
        _battleManager.ActionPresentationCompleted -= OnActionPresentationCompleted;
        _battleManager.BattleEnded -= OnBattleEnded;
    }

    private void OnCombatClicked()
    {
        if (_battleManager == null)
        {
            _battleManager = BattleManager.Instance;
            if (_battleManager == null)
            {
                return;
            }
        }

        _battleManager.IssuePlayerCommand(new PlayerBattleCommand(PlayerBattleCommandType.NormalAttack));
    }

    private void OnFleeClicked()
    {
        if (_battleManager == null)
        {
            _battleManager = BattleManager.Instance;
            if (_battleManager == null)
            {
                return;
            }
        }

        _battleManager.IssuePlayerCommand(new PlayerBattleCommand(PlayerBattleCommandType.Flee));
    }

    private void RefreshView()
    {
        if (_battleManager == null)
        {
            _battleManager = BattleManager.Instance;
            if (_battleManager == null)
            {
                return;
            }
        }

        if (enemyNameText != null)
        {
            enemyNameText.text = _battleManager.GetEnemyDisplayName();
        }

        if (hpText != null)
        {
            hpText.text = _battleManager.GetPlayerHpDisplay();
        }

        if (enemyHpText == null)
        {
            Debug.LogError("BattlePanel.RefreshView -> enemyHpText 未绑定，请在 Inspector 绑定敌方HP文本。");
        }
        else
        {
            enemyHpText.text = _battleManager.GetEnemyHpDisplay();
        }

        if (fleeRateText != null)
        {
            fleeRateText.text = _battleManager.GetFleeRateDisplay();
        }

        if (battleLogText == null)
        {
            Debug.LogError("BattlePanel.RefreshView -> battleLogText 未绑定，请在 Inspector 绑定战斗日志文本。");
        }

        if (transformButton != null)
        {
            transformButton.gameObject.SetActive(false);
        }

        if (controlsRoot != null)
        {
            controlsRoot.SetActive(_battleManager.ControlsVisible);
        }
        else
        {
            if (combatButton != null)
            {
                combatButton.gameObject.SetActive(_battleManager.ControlsVisible);
            }

            if (fleeButton != null)
            {
                fleeButton.gameObject.SetActive(_battleManager.ControlsVisible);
            }
        }

        if (combatButton != null)
        {
            combatButton.interactable = _battleManager.ControlsInteractable;
        }

        if (fleeButton != null)
        {
            fleeButton.interactable = _battleManager.ControlsInteractable;
        }
    }

    private void OnPlayerAttackResolved(BattleAttackEvent evt)
    {
        if (evt == null)
        {
            return;
        }

        _lastPlayerAttackLine = BuildAttackLine(evt);
        UpdateBattleLogText();
    }

    private void OnEnemyAttackResolved(BattleAttackEvent evt)
    {
        if (evt == null)
        {
            return;
        }

        _lastEnemyAttackLine = BuildAttackLine(evt);
        UpdateBattleLogText();
    }

    private void OnBattleEnded(BattleEndEvent evt)
    {
        // Battle result narration is handled by BattleManager (UIManager modal exception).
        // BattleLogText should only render attack lines; no action needed here.
    }

    private void OnBattleLogRaised(BattleLogEvent evt)
    {
        if (evt == null)
        {
            return;
        }

        if (evt.EventType == BattleLogEventType.ActionHint)
        {
            string message = evt.Message ?? string.Empty;
            if (string.IsNullOrWhiteSpace(message))
            {
                _lastPlayerAttackLine = string.Empty;
                _lastEnemyAttackLine = string.Empty;
            }
            else
            {
                _lastPlayerAttackLine = message;
            }
            UpdateBattleLogText();
            return;
        }

        if (evt.EventType == BattleLogEventType.SettlementHint)
        {
            _lastPlayerAttackLine = evt.Message ?? string.Empty;
            UpdateBattleLogText();
        }
    }

    private void OnTurnSubPhaseChanged(BattleTurnSubPhaseChangedEvent evt)
    {
        // Keep panel as passive renderer: sync visual state on phase transitions.
        RefreshView();
    }

    private void OnActionPresentationCompleted(BattleActionPresentationEvent evt)
    {
        // Reserved for future UI sync hooks when action presentation completes.
    }

    private void UpdateBattleLogText()
    {
        if (_battleLogTargets.Count == 0)
        {
            return;
        }

        string combined = BuildBattleLogOutput(_lastPlayerAttackLine, _lastEnemyAttackLine);
        if (string.IsNullOrWhiteSpace(combined))
        {
            for (int i = 0; i < _battleLogTargets.Count; i++)
            {
                TMP_Text target = _battleLogTargets[i];
                if (target != null)
                {
                    target.text = string.Empty;
                }
            }
            return;
        }

        for (int i = 0; i < _battleLogTargets.Count; i++)
        {
            TMP_Text target = _battleLogTargets[i];
            if (target != null)
            {
                target.text = combined;
            }
        }
    }

    private static string BuildAttackLine(BattleAttackEvent evt)
    {
        if (evt == null)
        {
            return string.Empty;
        }

        string attacker = evt.AttackerName ?? string.Empty;
        string defender = evt.DefenderName ?? string.Empty;
        string skill = evt.SkillLabel ?? string.Empty;
        string criticalTag = evt.IsCritical ? "【暴击】" : string.Empty;
        string blockTag = evt.IsBlocked ? "【格挡】" : string.Empty;
        string dodgeTag = evt.IsDodged ? "【闪避】" : string.Empty;
        string core = $"{attacker}使用了【{skill}】{criticalTag}，对{defender}造成{evt.Damage}点伤害{blockTag}{dodgeTag}";
        string attackerPhase = evt.AttackerPhaseLogSuffix ?? string.Empty;
        string defenderPhase = evt.DefenderPhaseLogSuffix ?? string.Empty;
        string afterAttackPhase = evt.AfterAttackPhaseLogSuffix ?? string.Empty;
        string line = core + "。";
        if (!string.IsNullOrEmpty(attackerPhase))
        {
            line += attackerPhase + "。";
        }

        if (!string.IsNullOrEmpty(defenderPhase))
        {
            line += defenderPhase + "。";
        }

        if (!string.IsNullOrEmpty(afterAttackPhase))
        {
            line += afterAttackPhase + "。";
        }

        return line;
    }

    private static string BuildBattleLogOutput(string playerActionOutput, string enemyActionOutput)
    {
        string playerOutput = playerActionOutput ?? string.Empty;
        string enemyOutput = enemyActionOutput ?? string.Empty;

        if (string.IsNullOrWhiteSpace(playerOutput))
        {
            return enemyOutput;
        }

        if (string.IsNullOrWhiteSpace(enemyOutput))
        {
            return playerOutput;
        }

        return $"{playerOutput}\n{enemyOutput}";
    }

    private void RebuildBattleLogTargets()
    {
        _battleLogTargets.Clear();
        if (battleLogText != null)
        {
            _battleLogTargets.Add(battleLogText);
        }

        TMP_Text[] candidates = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < candidates.Length; i++)
        {
            TMP_Text candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            string name = candidate.name;
            bool looksLikeBattleLog = name != null &&
                name.ToLowerInvariant().Contains("battlelog");
            if (!looksLikeBattleLog)
            {
                continue;
            }

            if (!_battleLogTargets.Contains(candidate))
            {
                _battleLogTargets.Add(candidate);
            }
        }
    }
}
