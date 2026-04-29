using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Networking.PlayerConnection;

//using System.Diagnostics;
using UnityEngine;

public enum GameRunState
{
    MainMenu = 0,
    InGame = 1
}

public class GameManager : MonoBehaviour
{
    private readonly SaveSnapshotAssembler _saveSnapshotAssembler = new SaveSnapshotAssembler();
    private const int MAX_PENDING_RUNTIME_INIT_RETRY_FRAMES = 8;

    public static GameManager Instance
    {
        get; private set;
    }

    public int HomeDistance
    {
        get
        {
            GlobalRulesConfig rules = GetGlobalRulesConfig();
            return rules != null ? rules.homeDistance : 0;
        }
    }

    public int GetRegionByDistance(int distanceKm)
    {
        if (RouteProgressManager.Instance != null)
        {
            return RouteProgressManager.Instance.GetCurrentMainRegionId();
        }
        return 0;
    }

    public event Action NewGameEvent;
    public event Action GameOverEvent;
    public GameRunState CurrentRunState { get; private set; } = GameRunState.MainMenu;
    public bool IsInGame => CurrentRunState == GameRunState.InGame;
    private bool _hasBootstrappedFromLaunchContext;
    private PlayerStateManager _boundPlayerStateManager;
    private Coroutine _pendingRuntimeInitCoroutine;
    private bool _hasBootstrapValidated;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BootstrapFromLaunchContext();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            InventoryManager.Instance.AddItem(2, 1);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            InventoryManager.Instance.RemoveItem(2, 2);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            InventoryPage.Instance.UpdateUIDisplay();
        }
    }

    private void OnDestroy()
    {
        UnbindPlayerHealthWatcher();
        UnbindRouteProgressEvents();
    }

    private void OnEnable()
    {
        BindRouteProgressEvents();
    }

    private void OnDisable()
    {
        UnbindRouteProgressEvents();
    }

    private void BindRouteProgressEvents()
    {
        if (RouteProgressManager.Instance == null) return;
        RouteProgressManager.Instance.OnDistanceChanged -= OnDistanceChanged;
        RouteProgressManager.Instance.OnDistanceChanged += OnDistanceChanged;
        RouteProgressManager.Instance.OnDayChanged -= OnDayChanged;
        RouteProgressManager.Instance.OnDayChanged += OnDayChanged;
    }

    private void UnbindRouteProgressEvents()
    {
        if (RouteProgressManager.Instance == null) return;
        RouteProgressManager.Instance.OnDistanceChanged -= OnDistanceChanged;
        RouteProgressManager.Instance.OnDayChanged -= OnDayChanged;
    }

    private void OnDistanceChanged(int newDistance)
    {
        CheckDistance();
    }

    private void OnDayChanged(int newDay)
    {
        CheckDay();
    }

    private void CheckDay()
    {
        if (RouteProgressManager.Instance != null && RouteProgressManager.Instance.GetDay() >= 180)
        {
            Debug.Log("时间不多了");
        }
    }

    private void CheckDistance()
    {
        if (RouteProgressManager.Instance != null &&
            RouteProgressManager.Instance.GetDistance() >= HomeDistance)
        {
            GameOver();
        }
    }

    private void CheckHealth()
    {
        if (PlayerStateManager.Instance != null &&
            PlayerStateManager.Instance.Current != null &&
            PlayerStateManager.Instance.CurrentHp <= 0f)
        {
            GameOver();
        }
    }

    private void BootstrapFromLaunchContext()
    {
        if (_hasBootstrappedFromLaunchContext)
        {
            return;
        }

        _hasBootstrappedFromLaunchContext = true;
        _hasBootstrapValidated = false;
        GameLaunchMode pendingMode = GameLaunchContext.PendingMode;
        string pendingStartPresetId = GameLaunchContext.ConsumePendingStartPresetId();
        SaveData saveData = GameLaunchContext.ConsumePendingSaveData();

        if (pendingMode == GameLaunchMode.Continue && saveData != null)
        {
            BootstrapContinueFromSaveData(saveData);
            return;
        }

        if (pendingMode == GameLaunchMode.Continue)
        {
            HandleContinueBootstrapFailure(
                "GameManager.BootstrapFromLaunchContext -> Continue 无有效存档数据，已中止继续。"
            );
            return;
        }

        if (pendingMode == GameLaunchMode.None)
        {
            return;
        }

        NewGame(pendingStartPresetId);
    }

    public void ApplyPendingLaunchContext()
    {
        _hasBootstrappedFromLaunchContext = false;
        BootstrapFromLaunchContext();
    }

    public bool TryBuildSaveData(out SaveData saveData, out string errorMessage)
    {
        saveData = null;
        errorMessage = string.Empty;

        try
        {
            saveData = _saveSnapshotAssembler.Assemble();
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"生成存档快照失败：{ex.Message}";
            return false;
        }
    }

    public void NewGame(string startPresetId = null)
    {
        ConfigRegistry configRegistry = AppRoot.Instance != null ? AppRoot.Instance.Configs : null;
        if (configRegistry == null)
        {
            Debug.LogError("GameManager.NewGame -> ConfigRegistry 未就绪。");
            return;
        }

        if (string.IsNullOrWhiteSpace(startPresetId))
        {
            Debug.LogError("GameManager.NewGame -> startPresetId 为空，已中止新开。");
            return;
        }

        StartGameConfig startGameConfig = configRegistry.FindStartGameConfigById(startPresetId);
        if (startGameConfig == null)
        {
            Debug.LogError($"GameManager.NewGame -> 未找到开局模板：{startPresetId}");
            return;
        }

        SessionRoot root = SessionRoot.EnsureExists();
        if (!root.InitializeFromDefault(startGameConfig, configRegistry.GlobalRules, out string playerInitErrorMessage))
        {
            Debug.LogError($"GameManager.NewGame -> 初始化玩家状态失败：{playerInitErrorMessage}");
            return;
        }

        StartPendingRuntimeInitializationCoroutine(
            root,
            "GameManager.NewGame",
            onSuccess: () =>
            {
                OnNewGameReady();
            },
            onFailedAfterRetries: () =>
            {
                OnNewGameReady();
            }
        );
    }

    private void BootstrapContinueFromSaveData(SaveData saveData)
    {
        SessionRoot root = SessionRoot.EnsureExists();
        if (!root.InitializeFromSave(saveData, GetGlobalRulesConfig(), out string runtimePrepareErrorMessage))
        {
            Debug.LogError(
                $"GameManager.ApplySaveData -> 运行时恢复准备失败，原因：{runtimePrepareErrorMessage}"
            );
            HandleContinueBootstrapFailure(
                "GameManager.BootstrapFromLaunchContext -> Continue 应用存档失败，已中止继续。"
            );
            return;
        }

        StartPendingRuntimeInitializationCoroutine(
            root,
            "GameManager.ApplySaveData",
            onSuccess: () =>
            {
                OnNewGameReady();
            },
            onFailedAfterRetries: () =>
            {
                HandleContinueBootstrapFailure(
                    "GameManager.BootstrapFromLaunchContext -> Continue 运行时初始化失败，已中止继续。"
                );
            }
        );
    }

    private void HandleContinueBootstrapFailure(string errorMessage)
    {
        Debug.LogError(errorMessage);
        if (AppRoot.Instance != null && AppRoot.Instance.SceneLoader != null)
        {
            AppRoot.Instance.SceneLoader.LoadMainMenu("MainMenuScene");
        }
    }

    private void StartPendingRuntimeInitializationCoroutine(
        SessionRoot root,
        string caller,
        Action onSuccess,
        Action onFailedAfterRetries
    )
    {
        if (_pendingRuntimeInitCoroutine != null)
        {
            StopCoroutine(_pendingRuntimeInitCoroutine);
            _pendingRuntimeInitCoroutine = null;
        }

        _pendingRuntimeInitCoroutine = StartCoroutine(
            TryApplyPendingRuntimeInitializationWithRetryCoroutine(
                root,
                caller,
                MAX_PENDING_RUNTIME_INIT_RETRY_FRAMES,
                onSuccess,
                onFailedAfterRetries
            )
        );
    }

    private IEnumerator TryApplyPendingRuntimeInitializationWithRetryCoroutine(
        SessionRoot root,
        string caller,
        int maxRetryFrames,
        Action onSuccess,
        Action onFailedAfterRetries
    )
    {
        if (root == null)
        {
            Debug.LogWarning($"{caller} -> SessionRoot 为空，无法应用运行时初始化。");
            onFailedAfterRetries?.Invoke();
            _pendingRuntimeInitCoroutine = null;
            yield break;
        }

        for (int retryFrame = 0; retryFrame <= maxRetryFrames; retryFrame++)
        {
            if (root.TryApplyPendingRuntimeInitialization(out string runtimeInitErrorMessage))
            {
                if (!TryValidateBootstrapOrAbort(caller))
                {
                    _pendingRuntimeInitCoroutine = null;
                    yield break;
                }
                onSuccess?.Invoke();
                _pendingRuntimeInitCoroutine = null;
                yield break;
            }

            if (retryFrame == maxRetryFrames)
            {
                Debug.LogWarning(
                    $"{caller} -> 运行时初始化在重试后仍未完成（{maxRetryFrames + 1} 帧），最后错误：{runtimeInitErrorMessage}"
                );
                onFailedAfterRetries?.Invoke();
                _pendingRuntimeInitCoroutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private bool TryValidateBootstrapOrAbort(string caller)
    {
        if (_hasBootstrapValidated)
        {
            return true;
        }

        if (RouteProgressManager.Instance == null)
        {
            Debug.LogError($"{caller} -> 启动校验失败：RouteProgressManager 未就绪。");
            if (AppRoot.Instance != null && AppRoot.Instance.SceneLoader != null)
            {
                AppRoot.Instance.SceneLoader.LoadMainMenu("MainMenuScene");
            }
            return false;
        }

        IReadOnlyList<MainRegionData> mainRegions = RouteProgressManager.Instance.GetAllMainRegionsForValidation();
        if (mainRegions == null || mainRegions.Count == 0)
        {
            Debug.LogError($"{caller} -> 启动校验失败：未加载到任何主地区配置。");
            if (AppRoot.Instance != null && AppRoot.Instance.SceneLoader != null)
            {
                AppRoot.Instance.SceneLoader.LoadMainMenu("MainMenuScene");
            }
            return false;
        }

        RegionLootTable lootTable = AdvanceFlowController.Instance != null ? AdvanceFlowController.Instance.RegionLootTable : null;
        try
        {
            RegionBootstrapValidator.ValidateOrThrow(
                mainRegions,
                EventManager.Instance,
                EnemyPoolService.Instance,
                lootTable
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"{caller} -> 启动校验异常：{ex.Message}");
            if (AppRoot.Instance != null && AppRoot.Instance.SceneLoader != null)
            {
                AppRoot.Instance.SceneLoader.LoadMainMenu("MainMenuScene");
            }
            return false;
        }

        _hasBootstrapValidated = true;
        return true;
    }

    public void GameOver()
    {
        GameOverEvent?.Invoke();
    }

    private void BindPlayerHealthWatcher()
    {
        UnbindPlayerHealthWatcher();
        if (PlayerStateManager.Instance == null)
        {
            return;
        }

        _boundPlayerStateManager = PlayerStateManager.Instance;
        _boundPlayerStateManager.PlayerRuntimeChanged -= OnPlayerRuntimeChanged;
        _boundPlayerStateManager.PlayerRuntimeChanged += OnPlayerRuntimeChanged;
    }

    private void UnbindPlayerHealthWatcher()
    {
        if (_boundPlayerStateManager != null)
        {
            _boundPlayerStateManager.PlayerRuntimeChanged -= OnPlayerRuntimeChanged;
            _boundPlayerStateManager = null;
        }
    }

    private void OnPlayerRuntimeChanged(PlayerRuntime runtime)
    {
        CheckHealth();
    }

    private GlobalRulesConfig GetGlobalRulesConfig()
    {
        return AppRoot.Instance != null && AppRoot.Instance.Configs != null
            ? AppRoot.Instance.Configs.GlobalRules
            : null;
    }

    private void OnNewGameReady()
    {
        SetRunState(GameRunState.InGame);
        BindPlayerHealthWatcher();
        NewGameEvent?.Invoke();
    }

    public void SetRunState(GameRunState state)
    {
        CurrentRunState = state;
    }
}
