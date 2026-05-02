using System;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private Action<BattleResult> _onBattleEnd;
    private CharacterRuntimeStats _playerSnapshot;
    private CharacterRuntimeStats _enemySnapshot;
    private CombatActionResult _pendingPlayerActionResult;
    private CombatActionResult _pendingEnemyActionResult;
    private Coroutine _turnRoutine;
    private bool _isEnding;
    private BattleResult? _pendingEndResult;
    private bool _controlsVisible = true;
    private bool _controlsInteractable = true;
    private BattlePhase _phase = BattlePhase.None;
    private BattleTurnSubPhase _turnSubPhase = BattleTurnSubPhase.None;
    private int _playerRuptureStackCount;

    public CharacterRuntimeStats PlayerRuntime => _playerSnapshot;
    public CharacterRuntimeStats EnemyRuntime => _enemySnapshot;
    public EnemyData EnemyTemplate => null;
    public BattlePhase Phase => _phase;
    public BattleTurnSubPhase TurnSubPhase => _turnSubPhase;
    public bool ControlsVisible => _controlsVisible;
    public bool ControlsInteractable => _controlsInteractable;

    public event Action<BattleAttackEvent> PlayerAttackResolved;
    public event Action<BattleAttackEvent> EnemyAttackResolved;
    public event Action<BattleLogEvent> BattleLogRaised;
    public event Action<BattleTurnSubPhaseChangedEvent> TurnSubPhaseChanged;
    public event Action<BattleActionPresentationEvent> ActionPresentationCompleted;
    public event Action<BattleEndEvent> BattleEnded;

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

    public void StartBattle(EnemyData enemy)
    {
        StartBattle(enemy, null);
    }

    public bool StartBattleByEnemyId(string currentRegionCode, string enemyId, Action<BattleResult> onEnd)
    {
        if (EnemyPoolService.Instance == null)
        {
            Debug.LogError("BattleManager.StartBattleByEnemyId -> EnemyPoolService 未挂载。");
            return false;
        }

        EnemyData enemy = EnemyPoolService.Instance.GetEnemyByIdInRegion(currentRegionCode, enemyId);
        StartBattle(enemy, onEnd);
        return true;
    }

    public void StartBattle(EnemyData enemy, Action<BattleResult> onEnd)
    {
        UIManager.Instance?.ClearEventNarrationModalText();

        if (_turnRoutine != null)
        {
            StopCoroutine(_turnRoutine);
            _turnRoutine = null;
        }

        _isEnding = false;
        _pendingEndResult = null;
        ClearPendingSettlementLogs();
        _onBattleEnd = onEnd;
        SetPhase(BattlePhase.Preparing);
        SetSubPhase(BattleTurnSubPhase.None, "BattleStart-Preparing");

        if (EnemyStateManager.Instance == null)
        {
            Debug.LogWarning("BattleManager.StartBattle -> EnemyStateManager 未就绪，无法开始战斗");
            EndBattle(BattleResult.None);
            return;
        }

        if (enemy == null)
        {
            Debug.LogWarning("敌人信息为空，无法开始战斗");
            EndBattle(BattleResult.None);
            return;
        }

        EnemyStateManager.Instance.BeginBattle(enemy);
        EnemyRuntime enemyRuntime = EnemyStateManager.Instance.Current;
        if (enemyRuntime == null)
        {
            Debug.LogWarning("BattleManager.StartBattle -> 敌人运行态为空，无法开始战斗");
            EndBattle(BattleResult.None);
            return;
        }

        if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
        {
            Debug.LogWarning("BattleManager.StartBattle -> 玩家信息为空，无法开始战斗");
            EndBattle(BattleResult.None);
            return;
        }

        _playerSnapshot = new CharacterRuntimeStats(
            PlayerStateManager.Instance.Current,
            PlayerStateManager.Instance.Current.TraitIds,
            PlayerStateManager.Instance.Current.EscapeRate
        );
        _enemySnapshot = new CharacterRuntimeStats(enemyRuntime);
        ResetBattleCombatExtras();

        SetSubPhase(BattleTurnSubPhase.WaitPlayerInput, "BattleStart-WaitPlayerInput");
        InjectTraits();

        if (UIManager.Instance == null)
        {
            Debug.LogError("BattleManager.StartBattle -> UIManager 未就绪，无法切换战斗沉浸式 UI。");
            EndBattle(BattleResult.None);
            return;
        }

        UIManager.Instance.OpenUIEntry(UIKey.HudStatus);
        UIManager.Instance.EnsureCombatStatsVisibleIfPlayerStatsVisible();
        UIManager.Instance.CloseUIEntry(UIKey.ActionBar);
        UIManager.Instance.OpenUIEntry(UIKey.Battle);
        UIManager.Instance.SetCombatStatsOpen(true);
    }

    public void EndBattle(BattleResult result)
    {
        if (_isEnding)
        {
            return;
        }

        _isEnding = true;
        _pendingEndResult = null;
        ClearPendingSettlementLogs();
        SetPhase(BattlePhase.Ended);
        SetSubPhase(BattleTurnSubPhase.None, "BattleEnd");

        if (_turnRoutine != null)
        {
            StopCoroutine(_turnRoutine);
            _turnRoutine = null;
        }

        SyncPlayerHealthBack();

        if (UIManager.Instance == null)
        {
            Debug.LogError("BattleManager.EndBattle -> UIManager 未就绪，无法恢复战斗外 UI。");
        }
        else
        {
            UIManager.Instance.CloseUIEntry(UIKey.Battle);
            UIManager.Instance.CloseUIEntry(UIKey.CombatStats);
            UIManager.Instance.OpenUIEntry(UIKey.HudStatus);
            UIManager.Instance.SetHudPlayerStatusVisible(true);
            UIManager.Instance.OpenUIEntry(UIKey.ActionBar);
            UIManager.Instance.UpdateInfo();
        }

        if (TraitManager.Instance != null)
        {
            TraitManager.Instance.ClearOwnerTraits(TraitOwner.Enemy);
        }

        _onBattleEnd?.Invoke(result);
        _onBattleEnd = null;
        _playerSnapshot = null;
        _enemySnapshot = null;
        ResetBattleCombatExtras();
        if (EnemyStateManager.Instance != null)
        {
            EnemyStateManager.Instance.ClearCurrent();
        }

        string narration = BuildBattleEndNarration(result);
        BattleEnded?.Invoke(new BattleEndEvent(result, narration));
        if (UIManager.Instance != null && !string.IsNullOrWhiteSpace(narration))
        {
            UIManager.Instance.ShowEventNarrationModal(narration);
        }

        SetPhase(BattlePhase.None);
        _isEnding = false;
    }

    public bool IssuePlayerCommand(PlayerBattleCommand command)
    {
        if (command == null || !CanPlayerOperate())
        {
            return false;
        }

        switch (command.CommandType)
        {
            case PlayerBattleCommandType.NormalAttack:
                _turnRoutine = StartCoroutine(PlayerNormalAttackFlow());
                return true;
            case PlayerBattleCommandType.Flee:
                _turnRoutine = StartCoroutine(PlayerFleeFlow());
                return true;
            default:
                return false;
        }
    }

    public string GetEnemyDisplayName()
    {
        EnemyRuntime enemyRuntime = EnemyStateManager.Instance != null ? EnemyStateManager.Instance.Current : null;
        if (enemyRuntime == null && _enemySnapshot == null)
        {
            return string.Empty;
        }

        string name = _enemySnapshot != null ? _enemySnapshot.Name : enemyRuntime.DisplayName;
        int level = enemyRuntime != null ? enemyRuntime.Level : 1;
        return $"{name} LV{level}";
    }

    private string GetEnemyBaseName()
    {
        EnemyRuntime enemyRuntime = EnemyStateManager.Instance != null ? EnemyStateManager.Instance.Current : null;
        if (enemyRuntime == null && _enemySnapshot == null)
        {
            return string.Empty;
        }

        string name = _enemySnapshot != null ? _enemySnapshot.Name : enemyRuntime.DisplayName;
        return string.IsNullOrWhiteSpace(name) ? string.Empty : name;
    }

    public string GetPlayerHpDisplay()
    {
        int hp = _playerSnapshot != null ? _playerSnapshot.CurrentHp : 0;
        int maxHp = _playerSnapshot != null ? _playerSnapshot.MaxHp : 0;
        return $"HP{hp}/{maxHp}";
    }

    public string GetEnemyHpDisplay()
    {
        int hp = _enemySnapshot != null ? _enemySnapshot.CurrentHp : 0;
        int maxHp = _enemySnapshot != null ? _enemySnapshot.MaxHp : 0;
        return $"HP{hp}/{maxHp}";
    }

    public string GetFleeRateDisplay()
    {
        float escapeRate = _playerSnapshot != null ? _playerSnapshot.EscapeRate : 0f;
        return $"{escapeRate:0.#}%";
    }

    private readonly struct BattleEndEvaluation
    {
        public BattleEndEvaluation(bool shouldQueue, BattleResult result, string source)
        {
            ShouldQueue = shouldQueue;
            Result = result;
            Source = source;
        }

        public bool ShouldQueue { get; }
        public BattleResult Result { get; }
        public string Source { get; }
    }

    private IEnumerator PlayerNormalAttackFlow()
    {
        SetSubPhase(BattleTurnSubPhase.PlayerAction, "PlayerNormalAttack-Action");
        ClearBattleLogForPlayerActionStart();
        ExecutePlayerActionByFactory(PlayerBattleCommandType.NormalAttack);
        SetSubPhase(BattleTurnSubPhase.PlayerSettlement, "PlayerNormalAttack-Settlement");
        UIManager.Instance?.RefreshActiveStatsPanelInHud();
        FlushSettlementLogs(BattleTurnSubPhase.PlayerSettlement);

        yield return WaitForSettlementPresentation(BattleActionActor.Player, BattleTurnSubPhase.PlayerSettlement);
        if (TryConsumePendingBattleEnd(BattleTurnSubPhase.PlayerSettlement, "PlayerNormalAttackFlow"))
        {
            _turnRoutine = null;
            yield break;
        }

        yield return EnemyNormalAttackFlow();

        if (CanReturnToPlayerTurn())
        {
            SetSubPhase(BattleTurnSubPhase.WaitPlayerInput, "RoundBackToPlayerInput");
        }

        _turnRoutine = null;
    }

    private IEnumerator PlayerFleeFlow()
    {
        SetSubPhase(BattleTurnSubPhase.PlayerAction, "PlayerFlee-Action");
        ClearBattleLogForPlayerActionStart();
        ExecutePlayerActionByFactory(PlayerBattleCommandType.Flee);
        string settlementReason = _pendingEndResult == BattleResult.Escape
            ? "PlayerFleeSuccess-Settlement"
            : "PlayerFleeFail-Settlement";
        SetSubPhase(BattleTurnSubPhase.PlayerSettlement, settlementReason);
        UIManager.Instance?.RefreshActiveStatsPanelInHud();
        FlushSettlementLogs(BattleTurnSubPhase.PlayerSettlement);
        yield return WaitForSettlementPresentation(BattleActionActor.Player, BattleTurnSubPhase.PlayerSettlement);
        if (TryConsumePendingBattleEnd(BattleTurnSubPhase.PlayerSettlement, "PlayerFleeFlow-Settlement"))
        {
            _turnRoutine = null;
            yield break;
        }

        yield return EnemyNormalAttackFlow();

        if (CanReturnToPlayerTurn())
        {
            SetSubPhase(BattleTurnSubPhase.WaitPlayerInput, "RoundBackToPlayerInput");
        }

        _turnRoutine = null;
    }

    private IEnumerator EnemyNormalAttackFlow()
    {
        if (_enemySnapshot == null || _playerSnapshot == null)
        {
            yield break;
        }

        SetSubPhase(BattleTurnSubPhase.EnemyAction, "EnemyNormalAttack-Action");
        CombatActionResult actionResult = BuildEnemyActionResult();
        _pendingEnemyActionResult = actionResult;
        ApplyEnemyActionResultEffects(actionResult);
        ApplyEnemyActionEndIntent(actionResult);
        string settlementReason = actionResult != null && actionResult.EndIntent == BattleResult.EnemyEscape
            ? "EnemyEscape-Settlement"
            : "EnemyNormalAttack-Settlement";
        SetSubPhase(BattleTurnSubPhase.EnemySettlement, settlementReason);
        UIManager.Instance?.RefreshActiveStatsPanelInHud();
        FlushSettlementLogs(BattleTurnSubPhase.EnemySettlement);
        yield return WaitForSettlementPresentation(BattleActionActor.Enemy, BattleTurnSubPhase.EnemySettlement);
        TryConsumePendingBattleEnd(BattleTurnSubPhase.EnemySettlement, "EnemyNormalAttackFlow");
    }

    private bool CanPlayerOperate()
    {
        if (_isEnding || _turnRoutine != null)
        {
            return false;
        }

        if (_turnSubPhase != BattleTurnSubPhase.WaitPlayerInput)
        {
            return false;
        }

        return _playerSnapshot != null && _enemySnapshot != null;
    }


    private bool QueueBattleEndByHpIfNeeded()
    {
        BattleEndEvaluation evaluation = EvaluateBattleEndByHp();
        return ApplyBattleEndEvaluation(evaluation);
    }

    private BattleEndEvaluation EvaluateBattleEndByHp()
    {
        if (_playerSnapshot == null || _enemySnapshot == null || _isEnding)
        {
            return new BattleEndEvaluation(false, BattleResult.None, "InvalidRuntimeState");
        }

        if (_enemySnapshot.CurrentHp <= 0)
        {
            return new BattleEndEvaluation(true, BattleResult.Win, "EnemyHpDepleted");
        }

        if (_playerSnapshot.CurrentHp <= 0)
        {
            return new BattleEndEvaluation(true, BattleResult.Lose, "PlayerHpDepleted");
        }

        return new BattleEndEvaluation(false, BattleResult.None, "NoEndCondition");
    }

    private bool ApplyBattleEndEvaluation(BattleEndEvaluation evaluation)
    {
        if (!evaluation.ShouldQueue || evaluation.Result == BattleResult.None)
        {
            return false;
        }

        QueueBattleEnd(evaluation.Result);
        Debug.Log($"[BattleManager] QueueBattleEnd -> {evaluation.Result} (Source: {evaluation.Source})");
        return true;
    }

    private void QueueBattleEnd(BattleResult result)
    {
        if (_isEnding || result == BattleResult.None)
        {
            return;
        }

        if (_pendingEndResult.HasValue)
        {
            return;
        }

        _pendingEndResult = result;
    }

    private void ExecutePlayerActionByFactory(PlayerBattleCommandType commandType)
    {
        _pendingPlayerActionResult = null;
        IPlayerAction action = PlayerActionFactory.Create(commandType);
        if (action == null)
        {
            return;
        }

        PlayerActionContext context = new PlayerActionContext(
            _playerSnapshot,
            _enemySnapshot,
            GetEnemyBaseName(),
            GetPlayerDamageBoostMultiplier(),
            GetEnemyDamageReductionMultiplier()
        );
        CombatActionResult result = action.Execute(context);
        _pendingPlayerActionResult = result;
        ApplyPlayerActionResultEffects(result);
        ApplyPlayerActionEndIntent(result);
    }

    private void ApplyPlayerActionResultEffects(CombatActionResult result)
    {
        if (result == null || result.Effects == null)
        {
            return;
        }

        for (int i = 0; i < result.Effects.Count; i++)
        {
            CombatActionEffect effect = result.Effects[i];
            if (effect == null)
            {
                continue;
            }

            switch (effect.EffectType)
            {
                case CombatActionEffectType.DamageEnemy:
                    if (_enemySnapshot != null)
                    {
                        _enemySnapshot.ApplyDamage(effect.Amount);
                    }
                    break;
            }
        }

        QueueBattleEndByHpIfNeeded();
    }

    private void ApplyPlayerActionEndIntent(CombatActionResult result)
    {
        if (result == null || result.EndIntent == BattleResult.None)
        {
            return;
        }

        QueueBattleEnd(result.EndIntent);
    }

    private CombatActionResult BuildEnemyActionResult()
    {
        CombatActionResult result = new CombatActionResult();
        if (_enemySnapshot == null || _playerSnapshot == null)
        {
            return result;
        }

        EnemyRuntime enemyRuntime = EnemyStateManager.Instance != null ? EnemyStateManager.Instance.Current : null;
        bool canEscape = enemyRuntime != null && enemyRuntime.CanEscape;
        if (canEscape)
        {
            float escapeRatePercent = enemyRuntime != null ? enemyRuntime.EscapeRatePercent : 0f;
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll <= escapeRatePercent)
            {
                result.EndIntent = BattleResult.EnemyEscape;
                return result;
            }
        }

        NormalAttackResolution resolution = NormalAttackResolver.Resolve(
            _enemySnapshot,
            _playerSnapshot,
            GetEnemyDamageBoostMultiplier(),
            GetPlayerDamageReductionMultiplier()
        );
        int damage = resolution.Damage;
        BattleItemHookRunner.RunPlayerBeforeReceiveHit(
            _enemySnapshot,
            _playerSnapshot,
            ref damage,
            out string enemyAttackerPhaseLog,
            out string playerDefenderPhaseLog);
        result.Effects.Add(new CombatActionEffect(CombatActionEffectType.DamagePlayer, damage));
        string enemyName = GetEnemyBaseName();
        result.SettlementLogs.Add(CombatSettlementLog.FromAttack(new BattleAttackEvent(
            enemyName,
            "你",
            "普攻",
            damage,
            resolution.IsCritical,
            resolution.IsBlocked,
            resolution.IsDodged,
            enemyAttackerPhaseLog,
            playerDefenderPhaseLog,
            null
        )));
        return result;
    }

    private static float GetPlayerDamageBoostMultiplier()
    {
        return 1f;
    }

    private static float GetEnemyDamageBoostMultiplier()
    {
        return 1f;
    }

    private static float GetPlayerDamageReductionMultiplier()
    {
        return 0f;
    }

    private static float GetEnemyDamageReductionMultiplier()
    {
        return 0f;
    }

    private void ApplyEnemyActionResultEffects(CombatActionResult result)
    {
        if (result == null || result.Effects == null)
        {
            return;
        }

        for (int i = 0; i < result.Effects.Count; i++)
        {
            CombatActionEffect effect = result.Effects[i];
            if (effect == null)
            {
                continue;
            }

            switch (effect.EffectType)
            {
                case CombatActionEffectType.DamagePlayer:
                    if (_playerSnapshot != null)
                    {
                        _playerSnapshot.ApplyDamage(effect.Amount);
                    }
                    break;
            }
        }

        QueueBattleEndByHpIfNeeded();
    }

    private void ApplyEnemyActionEndIntent(CombatActionResult result)
    {
        if (result == null || result.EndIntent == BattleResult.None)
        {
            return;
        }

        QueueBattleEnd(result.EndIntent);
    }

    private void FlushSettlementLogs(BattleTurnSubPhase settlementSubPhase)
    {
        if (_turnSubPhase != settlementSubPhase)
        {
            Debug.LogWarning($"[BattleManager] FlushSettlementLogs blocked at SubPhase {_turnSubPhase}, expected {settlementSubPhase}.");
            return;
        }

        if (settlementSubPhase == BattleTurnSubPhase.PlayerSettlement)
        {
            if (_pendingPlayerActionResult != null && _pendingPlayerActionResult.SettlementLogs != null)
            {
                for (int i = 0; i < _pendingPlayerActionResult.SettlementLogs.Count; i++)
                {
                    CombatSettlementLog settlementLog = _pendingPlayerActionResult.SettlementLogs[i];
                    if (settlementLog == null)
                    {
                        continue;
                    }

                    if (settlementLog.LogType == CombatSettlementLogType.Attack && settlementLog.AttackEvent != null)
                    {
                        PlayerAttackResolved?.Invoke(settlementLog.AttackEvent);
                        continue;
                    }

                    if (settlementLog.LogType == CombatSettlementLogType.Hint && !string.IsNullOrWhiteSpace(settlementLog.Message))
                    {
                        BattleLogRaised?.Invoke(new BattleLogEvent(BattleLogEventType.SettlementHint, settlementLog.Message));
                    }
                }
            }

            _pendingPlayerActionResult = null;
            return;
        }

        if (settlementSubPhase == BattleTurnSubPhase.EnemySettlement)
        {
            if (_pendingEnemyActionResult != null && _pendingEnemyActionResult.SettlementLogs != null)
            {
                for (int i = 0; i < _pendingEnemyActionResult.SettlementLogs.Count; i++)
                {
                    CombatSettlementLog settlementLog = _pendingEnemyActionResult.SettlementLogs[i];
                    if (settlementLog == null)
                    {
                        continue;
                    }

                    if (settlementLog.LogType == CombatSettlementLogType.Attack && settlementLog.AttackEvent != null)
                    {
                        EnemyAttackResolved?.Invoke(settlementLog.AttackEvent);
                        continue;
                    }

                    if (settlementLog.LogType == CombatSettlementLogType.Hint && !string.IsNullOrWhiteSpace(settlementLog.Message))
                    {
                        BattleLogRaised?.Invoke(new BattleLogEvent(BattleLogEventType.SettlementHint, settlementLog.Message));
                    }
                }
            }

            _pendingEnemyActionResult = null;
        }
    }

    private void ClearPendingSettlementLogs()
    {
        _pendingPlayerActionResult = null;
        _pendingEnemyActionResult = null;
    }

    private void ClearBattleLogForPlayerActionStart()
    {
        BattleLogRaised?.Invoke(new BattleLogEvent(BattleLogEventType.ActionHint, string.Empty));
    }

    private bool TryConsumePendingBattleEnd(BattleTurnSubPhase settlementSubPhase, string source)
    {
        if (_isEnding || !_pendingEndResult.HasValue)
        {
            return false;
        }

        if (!CanConsumePendingBattleEndInCurrentStage(settlementSubPhase, source))
        {
            return false;
        }

        BattleResult result = _pendingEndResult.Value;
        _pendingEndResult = null;
        EndBattle(result);
        return true;
    }

    private bool CanConsumePendingBattleEndInCurrentStage(BattleTurnSubPhase settlementSubPhase, string source)
    {
        bool isSettlementStage = _turnSubPhase == BattleTurnSubPhase.PlayerSettlement
            || _turnSubPhase == BattleTurnSubPhase.EnemySettlement;
        bool isExpectedSettlement = _turnSubPhase == settlementSubPhase;
        if (isSettlementStage && isExpectedSettlement)
        {
            return true;
        }

        Debug.LogWarning(
            $"[BattleManager] TryConsumePendingBattleEnd blocked at SubPhase {_turnSubPhase}, expected {settlementSubPhase}. Source: {source}");
        return false;
    }

    private IEnumerator WaitForSettlementPresentation(BattleActionActor actor, BattleTurnSubPhase expectedSettlementSubPhase)
    {
        if (_turnSubPhase != expectedSettlementSubPhase)
        {
            yield break;
        }

        yield return new WaitForSeconds(1f);
        ActionPresentationCompleted?.Invoke(new BattleActionPresentationEvent(actor, _phase));
    }

    private bool CanReturnToPlayerTurn()
    {
        if (_pendingEndResult.HasValue)
        {
            return false;
        }

        if (_phase == BattlePhase.None || _phase == BattlePhase.Ended)
        {
            return false;
        }

        return _playerSnapshot != null && _enemySnapshot != null;
    }

    private static string BuildBattleEndNarration(BattleResult result)
    {
        switch (result)
        {
            case BattleResult.Win:
                return "战斗胜利。";
            case BattleResult.Lose:
                return "战斗失败。";
            case BattleResult.Escape:
                return "你成功逃离了战斗。";
            case BattleResult.EnemyEscape:
                return "敌人逃离了战斗。";
            default:
                return string.Empty;
        }
    }

    private void SyncPlayerHealthBack()
    {
        if (_playerSnapshot == null || PlayerStateManager.Instance == null)
        {
            return;
        }

        PlayerStateManager.Instance.CurrentHp = _playerSnapshot.CurrentHp;
    }

    private void SetPhase(BattlePhase phase)
    {
        _phase = phase;
        ApplyControlsByPhase(phase);
    }

    private void SetSubPhase(BattleTurnSubPhase subPhase, string reason)
    {
        BattleTurnSubPhase fromSubPhase = _turnSubPhase;
        _turnSubPhase = subPhase;
        BattlePhase mappedPhase = MapSubPhaseToLegacyPhase(subPhase);
        _phase = mappedPhase;
        ApplyControlsByPhase(mappedPhase);
        TurnSubPhaseChanged?.Invoke(new BattleTurnSubPhaseChangedEvent(fromSubPhase, subPhase, reason));
        Debug.Log($"[BattleManager] SubPhase -> {subPhase} (LegacyPhase: {mappedPhase}, Reason: {reason})");
    }

    private static BattlePhase MapSubPhaseToLegacyPhase(BattleTurnSubPhase subPhase)
    {
        switch (subPhase)
        {
            case BattleTurnSubPhase.WaitPlayerInput:
                return BattlePhase.PlayerTurn;
            case BattleTurnSubPhase.PlayerAction:
            case BattleTurnSubPhase.PlayerSettlement:
            case BattleTurnSubPhase.EnemySettlement:
                return BattlePhase.Resolving;
            case BattleTurnSubPhase.EnemyAction:
                return BattlePhase.EnemyTurn;
            default:
                return BattlePhase.None;
        }
    }

    private void ApplyControlsByPhase(BattlePhase phase)
    {
        bool isPlayerTurn = phase == BattlePhase.PlayerTurn;
        _controlsVisible = isPlayerTurn;
        _controlsInteractable = isPlayerTurn;
    }

    private void InjectTraits()
    {
        if (TraitManager.Instance == null)
        {
            return;
        }

        TraitManager.Instance.ClearOwnerTraits(TraitOwner.Enemy);
        TraitManager.Instance.ClearOwnerTraits(TraitOwner.Player);

        EnemyRuntime enemyRuntime = EnemyStateManager.Instance != null && EnemyStateManager.Instance.Current != null
            ? EnemyStateManager.Instance.Current
            : null;
        if (enemyRuntime != null && enemyRuntime.TraitIds != null)
        {
            foreach (string traitId in enemyRuntime.TraitIds)
            {
                TraitManager.Instance.AddTrait(traitId, TraitOwner.Enemy);
            }
        }

        PlayerRuntime playerRuntime = PlayerStateManager.Instance != null && PlayerStateManager.Instance.Current != null
            ? PlayerStateManager.Instance.Current
            : null;
        if (playerRuntime != null && playerRuntime.TraitIds != null)
        {
            foreach (string traitId in playerRuntime.TraitIds)
            {
                TraitManager.Instance.AddTrait(traitId, TraitOwner.Player);
            }
        }
    }

    private void ResetBattleCombatExtras()
    {
        _playerRuptureStackCount = 0;
    }

    public int AdvancePlayerRuptureStackAndGetCapped()
    {
        _playerRuptureStackCount = Mathf.Min(_playerRuptureStackCount + 1, 20);
        return _playerRuptureStackCount;
    }
}

public enum BattleResult
{
    None = 0,
    Win = 1,
    Lose = 2,
    Escape = 3,
    EnemyEscape = 4
}

public enum BattlePhase
{
    None = 0,
    Preparing = 1,
    PlayerTurn = 2,
    EnemyTurn = 3,
    Resolving = 4,
    Ended = 5
}
