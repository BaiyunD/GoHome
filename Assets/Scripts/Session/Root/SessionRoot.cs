using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SessionRoot : MonoBehaviour
{
    private enum RuntimeInitMode
    {
        None,
        NewGame,
        Continue
    }

    public static SessionRoot Instance
    {
        get; private set;
    }

    public PlayerStateManager PlayerStateManager
    {
        get; private set;
    }

    public PlayerResourceService PlayerResourceService
    {
        get; private set;
    }
    public EnemyStateManager EnemyStateManager
    {
        get; private set;
    }
    public SurvivalResourceManager SurvivalResourceManager
    {
        get; private set;
    }
    public ShopService ShopService
    {
        get; private set;
    }
    public ShopToastService ShopToastService
    {
        get; private set;
    }

    private RuntimeInitMode _pendingRuntimeInitMode = RuntimeInitMode.None;
    private SaveData _pendingRuntimeSaveData;
    private StartGameConfig _pendingRuntimeStartGameConfig;
    private GlobalRulesConfig _pendingRuntimeGlobalRulesConfig;
    private bool _hasAppliedPendingRuntimeInit;

    public static SessionRoot EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        SessionRoot existing = FindFirstObjectByType<SessionRoot>();
        if (existing != null)
        {
            return existing;
        }

        GameObject go = new GameObject("SessionRoot");
        return go.AddComponent<SessionRoot>();
    }

    public static void DestroyCurrent()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            return;
        }

        SessionRoot existing = FindFirstObjectByType<SessionRoot>();
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerStateManager = GetComponent<PlayerStateManager>();
        if (PlayerStateManager == null)
        {
            PlayerStateManager = gameObject.AddComponent<PlayerStateManager>();
        }

        PlayerResourceService = GetComponent<PlayerResourceService>();
        if (PlayerResourceService == null)
        {
            PlayerResourceService = gameObject.AddComponent<PlayerResourceService>();
        }

        EnemyStateManager = GetComponent<EnemyStateManager>();
        if (EnemyStateManager == null)
        {
            EnemyStateManager = gameObject.AddComponent<EnemyStateManager>();
        }

        SurvivalResourceManager = GetComponent<SurvivalResourceManager>();
        if (SurvivalResourceManager == null)
        {
            SurvivalResourceManager = gameObject.AddComponent<SurvivalResourceManager>();
        }

        ShopService = GetComponent<ShopService>();
        if (ShopService == null)
        {
            ShopService = gameObject.AddComponent<ShopService>();
        }

        ShopToastService = GetComponent<ShopToastService>();
        if (ShopToastService == null)
        {
            ShopToastService = gameObject.AddComponent<ShopToastService>();
        }

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool InitializeFromDefault(StartGameConfig startGameConfig, GlobalRulesConfig globalRulesConfig, out string errorMessage)
    {
        if (PlayerStateManager == null)
        {
            errorMessage = "SessionRoot.InitializeFromDefault -> PlayerStateManager 未就绪。";
            return false;
        }

        if (startGameConfig == null || startGameConfig.PlayerTemplate == null)
        {
            errorMessage = "SessionRoot.InitializeFromDefault -> StartGameConfig 或 PlayerTemplate 为空。";
            return false;
        }

        if (!PlayerStateManager.NewGame(startGameConfig, out errorMessage))
        {
            return false;
        }
        if (SurvivalResourceManager != null)
        {
            SurvivalResourceManager.InitializeFromStartConfig(startGameConfig);
        }
        InitializeRuntimeForNewGame(startGameConfig, globalRulesConfig);
        errorMessage = string.Empty;
        return true;
    }

    public bool InitializeFromSave(SaveData saveData, GlobalRulesConfig globalRulesConfig, out string errorMessage)
    {
        if (saveData == null)
        {
            errorMessage = "SessionRoot.InitializeFromSave -> 存档为空。";
            return false;
        }

        if (saveData.Player == null)
        {
            errorMessage = "SessionRoot.InitializeFromSave -> 存档缺少 Player 数据。";
            return false;
        }

        if (saveData.Route == null || saveData.Inventory == null || saveData.Traits == null || saveData.Items == null)
        {
            errorMessage = "SessionRoot.InitializeFromSave -> 存档缺少 Route/Inventory/Traits/Items 关键数据。";
            return false;
        }

        if (PlayerStateManager == null)
        {
            errorMessage = "SessionRoot.InitializeFromSave -> PlayerStateManager 未就绪。";
            return false;
        }

        bool applied = PlayerStateManager.ApplySaveSnapshot(saveData.Player, out errorMessage);
        if (!applied)
        {
            return false;
        }

        if (SurvivalResourceManager != null)
        {
            SurvivalResourceManager.ApplySnapshot(saveData.Player);
        }

        return InitializeRuntimeForSave(saveData, globalRulesConfig, out errorMessage);
    }

    public void InitializeRuntimeForNewGame(StartGameConfig startGameConfig, GlobalRulesConfig globalRulesConfig)
    {
        _pendingRuntimeStartGameConfig = startGameConfig;
        _pendingRuntimeGlobalRulesConfig = globalRulesConfig;
        _pendingRuntimeSaveData = null;
        _pendingRuntimeInitMode = RuntimeInitMode.NewGame;
        _hasAppliedPendingRuntimeInit = false;
    }

    public bool InitializeRuntimeForSave(SaveData saveData, GlobalRulesConfig globalRulesConfig, out string errorMessage)
    {
        if (saveData == null)
        {
            errorMessage = "SessionRoot.InitializeRuntimeForSave -> saveData 为空。";
            return false;
        }

        if (saveData.Route == null || saveData.Inventory == null || saveData.Traits == null || saveData.Items == null)
        {
            errorMessage = "SessionRoot.InitializeRuntimeForSave -> saveData 缺少 Route/Inventory/Traits/Items。";
            return false;
        }

        _pendingRuntimeStartGameConfig = null;
        _pendingRuntimeGlobalRulesConfig = globalRulesConfig;
        _pendingRuntimeSaveData = saveData;
        _pendingRuntimeInitMode = RuntimeInitMode.Continue;
        _hasAppliedPendingRuntimeInit = false;
        errorMessage = string.Empty;
        return true;
    }

    public bool TryApplyPendingRuntimeInitialization(out string errorMessage)
    {
        errorMessage = string.Empty;
        if (_pendingRuntimeInitMode == RuntimeInitMode.None || _hasAppliedPendingRuntimeInit)
        {
            return true;
        }

        if (!TryEnsureRuntimeManagersReady(out errorMessage))
        {
            return false;
        }

        switch (_pendingRuntimeInitMode)
        {
            case RuntimeInitMode.NewGame:
                ApplyNewGameRuntimeDefaults();
                break;
            case RuntimeInitMode.Continue:
                ApplyContinueRuntimeState();
                break;
            default:
                break;
        }

        _hasAppliedPendingRuntimeInit = true;
        _pendingRuntimeInitMode = RuntimeInitMode.None;
        _pendingRuntimeSaveData = null;
        _pendingRuntimeStartGameConfig = null;
        _pendingRuntimeGlobalRulesConfig = null;
        return true;
    }

    private bool TryEnsureRuntimeManagersReady(out string errorMessage)
    {
        List<string> missingManagers = new List<string>();
        if (RouteProgressManager.Instance == null)
        {
            missingManagers.Add(nameof(RouteProgressManager));
        }
        if (InventoryManager.Instance == null)
        {
            missingManagers.Add(nameof(InventoryManager));
        }
        if (TraitManager.Instance == null)
        {
            missingManagers.Add(nameof(TraitManager));
        }
        if (ShopService.Instance == null)
        {
            missingManagers.Add(nameof(ShopService));
        }

        if (missingManagers.Count == 0)
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = $"SessionRoot.TryApplyPendingRuntimeInitialization -> 运行时管理器未就绪：{string.Join(", ", missingManagers)}";
        Debug.LogWarning(errorMessage);
        return false;
    }

    private void ApplyNewGameRuntimeDefaults()
    {
        RouteProgressManager.Instance.Initialize(0, 1);
        ApplyStartInventory(_pendingRuntimeStartGameConfig);
        TraitManager.Instance.ReplacePlayerTraits(Array.Empty<string>());
        ConsumableRuntimeState.ResetForNewGame();
        if (ShopService.Instance != null)
        {
            ShopService.Instance.InitializeForNewGame();
        }
    }

    private void ApplyContinueRuntimeState()
    {
        SaveRouteData routeData = _pendingRuntimeSaveData != null ? _pendingRuntimeSaveData.Route : null;
        if (routeData != null)
        {
            RouteProgressManager.Instance.ApplyProgressSnapshot(
                routeData.Distance,
                routeData.Day,
                routeData.MainRegionId,
                routeData.SubRegionId
            );
        }
        else
        {
            RouteProgressManager.Instance.Initialize(0, 1);
        }

        SaveInventoryEntryData[] inventoryEntries = _pendingRuntimeSaveData != null && _pendingRuntimeSaveData.Inventory != null
            ? _pendingRuntimeSaveData.Inventory.Entries
            : Array.Empty<SaveInventoryEntryData>();
        InventoryManager.Instance.ReplaceAllItems(inventoryEntries, false);
        TriggerPassiveRebuildAfterInventoryReady();

        string[] playerTraitIds = _pendingRuntimeSaveData != null && _pendingRuntimeSaveData.Traits != null
            ? _pendingRuntimeSaveData.Traits.PlayerTraitIds
            : Array.Empty<string>();
        TraitManager.Instance.ReplacePlayerTraits(playerTraitIds);

        SaveConsumableData consumableData = _pendingRuntimeSaveData != null &&
            _pendingRuntimeSaveData.Items != null
            ? _pendingRuntimeSaveData.Items.Consumables
            : null;
        ConsumableRuntimeState.ApplySnapshot(consumableData);

        SaveShopData shopData = _pendingRuntimeSaveData != null ? _pendingRuntimeSaveData.Shop : null;
        ShopWalletSnapshot shopSnapshot = new ShopWalletSnapshot
        {
            Points = shopData != null ? shopData.Points : 0,
            BuyPriceSnapshots = SaveSnapshotAssembler.MapBuyPriceSnapshots(shopData)
        };
        if (ShopService.Instance != null)
        {
            ShopService.Instance.ApplySnapshot(shopSnapshot);
        }
    }

    private static void TriggerPassiveRebuildAfterInventoryReady()
    {
        PassiveSystem passiveSystem = FindFirstObjectByType<PassiveSystem>();
        if (passiveSystem != null)
        {
            passiveSystem.Rebuild();
        }
    }

    private static void ApplyStartInventory(StartGameConfig startGameConfig)
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        StartInventoryEntry[] entries = startGameConfig != null
            ? startGameConfig.StartInventory
            : Array.Empty<StartInventoryEntry>();
        if (entries == null || entries.Length == 0)
        {
            InventoryManager.Instance.ReplaceAllItems(Array.Empty<SaveInventoryEntryData>());
            return;
        }

        List<SaveInventoryEntryData> mappedEntries = new List<SaveInventoryEntryData>();
        for (int i = 0; i < entries.Length; i++)
        {
            StartInventoryEntry entry = entries[i];
            if (entry == null || entry.count <= 0)
            {
                continue;
            }

            mappedEntries.Add(new SaveInventoryEntryData
            {
                ItemId = entry.itemId,
                Count = entry.count
            });
        }

        InventoryManager.Instance.ReplaceAllItems(mappedEntries);
    }

}

