using UnityEngine;

public sealed class AppRoot : MonoBehaviour
{
    public static AppRoot Instance
    {
        get; private set;
    }

    public SaveSystem SaveSystem
    {
        get; private set;
    }

    public SceneLoader SceneLoader
    {
        get; private set;
    }

    public ConfigRegistry Configs
    {
        get; private set;
    }
    private bool _hasQuitAutoSaved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystem = new SaveSystem();
        Configs = new ConfigRegistry();
        SceneLoader = new SceneLoader(SaveSystem);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        if (_hasQuitAutoSaved)
        {
            return;
        }

        _hasQuitAutoSaved = true;
        if (GameManager.Instance == null || !GameManager.Instance.IsInGame)
        {
            return;
        }

        if (!GameManager.Instance.TryBuildSaveData(out SaveData saveData, out string errorMessage))
        {
            Debug.LogError($"AppRoot.OnApplicationQuit -> 自动保存失败：{errorMessage}");
            return;
        }

        if (SaveSystem == null)
        {
            Debug.LogError("AppRoot.OnApplicationQuit -> 自动保存失败：SaveSystem 未就绪。");
            return;
        }

        try
        {
            SaveSystem.Save(saveData);
            Debug.Log("AppRoot.OnApplicationQuit -> 已自动保存。");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AppRoot.OnApplicationQuit -> 自动保存异常：{ex.Message}");
        }
    }
}

