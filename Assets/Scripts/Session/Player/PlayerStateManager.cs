using System;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance
    {
        get; private set;
    }

    [Header("可选：玩家模板（仅用于 NewGame 初始化）")]
    [SerializeField] private PlayerData playerTemplate;
    private StartGameConfig _currentStartGameConfig;

    public PlayerRuntime Current
    {
        get; private set;
    }

    public StartGameConfig CurrentStartGameConfig => _currentStartGameConfig;
    public float CurrentHp
    {
        get
        {
            return Current != null ? Current.CurrentHp : 0f;
        }
        set
        {
            if (Current == null)
            {
                return;
            }

            float safeMax = Mathf.Max(0f, Current.MaxHp);
            Current.CurrentHp = Mathf.Clamp(value, 0f, safeMax);
        }
    }

    public event Action<PlayerRuntime> PlayerRuntimeChanged;

    public void SetPlayerTemplate(PlayerData template)
    {
        playerTemplate = template;
    }

    public void SetStartGameConfig(StartGameConfig startGameConfig)
    {
        _currentStartGameConfig = startGameConfig;
        if (_currentStartGameConfig != null && _currentStartGameConfig.PlayerTemplate != null)
        {
            playerTemplate = _currentStartGameConfig.PlayerTemplate;
        }
    }

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

    public bool NewGame(StartGameConfig startGameConfig, out string errorMessage)
    {
        if (startGameConfig == null || startGameConfig.PlayerTemplate == null)
        {
            errorMessage = "PlayerStateManager.NewGame -> StartGameConfig 或 PlayerTemplate 为空。";
            return false;
        }

        SetStartGameConfig(startGameConfig);
        PlayerData runtimeData = Instantiate(startGameConfig.PlayerTemplate);
        StartResourceOverrides startResources = _currentStartGameConfig != null
            ? _currentStartGameConfig.StartResources
            : null;
        Current = new PlayerRuntime(runtimeData, startResources);
        if (!ResetToNewGame(out errorMessage))
        {
            return false;
        }

        PlayerRuntimeChanged?.Invoke(Current);
        errorMessage = string.Empty;
        return true;
    }

    public bool ApplySaveSnapshot(SavePlayerData snapshot, int saveFormatVersion, out string errorMessage)
    {
        if (snapshot == null)
        {
            errorMessage = "玩家存档数据为空。";
            return false;
        }

        if (Current == null)
        {
            Current = PlayerRuntime.CreateForLoadBeforeSnapshot();
        }

        Current.CombatItemPassive = default;
        Current.ApplyCombatBaseFromSaveSnapshot(snapshot, saveFormatVersion);
        PlayerRuntimeChanged?.Invoke(Current);
        errorMessage = string.Empty;
        return true;
    }

    private bool ResetToNewGame(out string errorMessage)
    {
        if (Current == null)
        {
            errorMessage = "PlayerStateManager.ResetToNewGame -> Current 为空。";
            return false;
        }

        return Current.ResetFromStartConfig(_currentStartGameConfig, out errorMessage);
    }

}

