using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneLoader
{
    private readonly SaveSystem _saveSystem;

    public SceneLoader(SaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    public void LoadMainMenu(string sceneName)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllGameUiImmediate();
        }

        EnsureSceneLoaded(sceneName);
        SetSceneUiVisibility(sceneName, true);
        SetSceneUiVisibility(GetGameSceneName(), false);
        SetActiveSceneIfLoaded(sceneName);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetRunState(GameRunState.MainMenu);
        }
        if (MainMenuUIManager.Instance != null)
        {
            MainMenuUIManager.Instance.ResetToDefaultPage();
        }
        MainMenuController mainMenuController = Object.FindFirstObjectByType<MainMenuController>();
        if (mainMenuController != null)
        {
            mainMenuController.RefreshOnMenuShown();
        }
        ResolveAudioListenerConflicts(sceneName);
    }

    public void StartNewGame(string gameSceneName, string startPresetId)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllGameUiImmediate();
        }
        if (MainMenuUIManager.Instance != null)
        {
            MainMenuUIManager.Instance.CloseAllPagesByStackOrder();
        }

        GameLaunchContext.PrepareNewGame(startPresetId);
        EnsureSceneLoaded(gameSceneName);
        SetSceneUiVisibility(gameSceneName, true);
        SetSceneUiVisibility(GetMainMenuSceneName(), false);
        SetActiveSceneIfLoaded(gameSceneName);
        ApplyPendingLaunchContextIfPossible();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetRunState(GameRunState.InGame);
        }
        ResolveAudioListenerConflicts(gameSceneName);
    }

    public bool ContinueGame(string gameSceneName)
    {
        if (!_saveSystem.HasSave())
        {
            Debug.LogError("SceneLoader.ContinueGame -> 未检测到存档文件。");
            return false;
        }

        SaveData save = _saveSystem.Load();
        if (save == null)
        {
            Debug.LogError("SceneLoader.ContinueGame -> 存档读取失败或已损坏。");
            return false;
        }

        if (save.Player == null)
        {
            Debug.LogError("SceneLoader.ContinueGame -> 存档缺少 Player 字段。");
            return false;
        }

        if (MainMenuUIManager.Instance != null)
        {
            MainMenuUIManager.Instance.CloseAllPagesByStackOrder();
        }

        GameLaunchContext.PrepareContinue(save);
        EnsureSceneLoaded(gameSceneName);
        SetSceneUiVisibility(gameSceneName, true);
        SetSceneUiVisibility(GetMainMenuSceneName(), false);
        SetActiveSceneIfLoaded(gameSceneName);
        ApplyPendingLaunchContextIfPossible();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetRunState(GameRunState.InGame);
        }
        ResolveAudioListenerConflicts(gameSceneName);
        return true;
    }

    private static string GetMainMenuSceneName()
    {
        return "MainMenuScene";
    }

    private static string GetGameSceneName()
    {
        return "GameScene";
    }

    private static void EnsureSceneLoaded(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            return;
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    private static void SetSceneUiVisibility(string sceneName, bool visible)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null)
            {
                continue;
            }

            if (!IsUiRoot(root))
            {
                continue;
            }

            root.SetActive(visible);
        }
    }

    private static bool IsUiRoot(GameObject root)
    {
        if (root.GetComponent<Canvas>() != null)
        {
            return true;
        }

        if (root.GetComponentInChildren<Canvas>(true) != null)
        {
            return true;
        }

        if (root.GetComponent<GraphicRaycaster>() != null)
        {
            return true;
        }

        return false;
    }

    private static void SetActiveSceneIfLoaded(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        SceneManager.SetActiveScene(scene);
    }

    private static void ApplyPendingLaunchContextIfPossible()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.ApplyPendingLaunchContext();
    }

    private static void ResolveAudioListenerConflicts(string preferredSceneName)
    {
        AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>(true);
        AudioListener preferredListener = null;
        AudioListener fallbackListener = null;

        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener listener = listeners[i];
            if (listener == null || !listener.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (fallbackListener == null)
            {
                fallbackListener = listener;
            }

            if (listener.gameObject.scene.name == preferredSceneName && preferredListener == null)
            {
                preferredListener = listener;
            }
        }

        AudioListener keep = preferredListener != null ? preferredListener : fallbackListener;
        if (keep == null)
        {
            return;
        }

        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener listener = listeners[i];
            if (listener == null)
            {
                continue;
            }

            bool shouldEnable = listener == keep && listener.gameObject.activeInHierarchy;
            if (listener.enabled != shouldEnable)
            {
                listener.enabled = shouldEnable;
            }
        }
    }
}

