using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Transform startPresetButtonContainer;
    [SerializeField] private Button startPresetButtonPrefab;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text confirmMessageText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string gameSceneName = "GameScene";

    private const string NEW_GAME_CONFIRM_TEXT = "新开存档会删除存档，你确定要新开吗？";
    private const string CONTINUE_FAILED_TEXT = "继续失败：存档读取失败，请尝试新开。";
    private const string PRESET_LOAD_FAILED_TEXT = "开局模板加载失败：未找到可用模板。";
    private readonly List<Button> _generatedPresetButtons = new List<Button>();
    private bool _pendingDefaultPageReset;

    private void Awake()
    {
        if (confirmMessageText != null)
        {
            confirmMessageText.text = NEW_GAME_CONFIRM_TEXT;
        }
    }

    private void OnEnable()
    {
        MainMenuUIManager uiManager = MainMenuUIManager.Instance;
        if (uiManager != null)
        {
            uiManager.ResetToDefaultPage();
            _pendingDefaultPageReset = false;
        }
        else
        {
            _pendingDefaultPageReset = true;
        }
        ClearGeneratedPresetButtons();
        RefreshContinueState();
        SetFeedback(string.Empty);
    }

    private void Start()
    {
        if (!_pendingDefaultPageReset || MainMenuUIManager.Instance == null)
        {
            return;
        }

        MainMenuUIManager.Instance.ResetToDefaultPage();
        _pendingDefaultPageReset = false;
    }

    public void RefreshOnMenuShown()
    {
        RefreshContinueState();
        SetFeedback(string.Empty);
    }

    public void OnClickContinue()
    {
        SaveSystem saveSystem = AppRoot.Instance != null ? AppRoot.Instance.SaveSystem : null;
        if (saveSystem == null || !saveSystem.HasSave())
        {
            RefreshContinueState();
            return;
        }

        if (AppRoot.Instance == null || AppRoot.Instance.SceneLoader == null)
        {
            SetFeedback(CONTINUE_FAILED_TEXT);
            return;
        }

        bool success = AppRoot.Instance.SceneLoader.ContinueGame(gameSceneName);
        if (!success)
        {
            SetFeedback(CONTINUE_FAILED_TEXT);
            return;
        }
    }

    public void OnClickNewGame()
    {
        SaveSystem saveSystem = AppRoot.Instance != null ? AppRoot.Instance.SaveSystem : null;
        if (saveSystem != null && saveSystem.HasSave())
        {
            ShowConfirmPage();
            return;
        }

        ShowStartPresetPage();
    }

    public void OnClickConfirmNewGame()
    {
        SaveSystem saveSystem = AppRoot.Instance != null ? AppRoot.Instance.SaveSystem : null;
        if (saveSystem != null && saveSystem.HasSave())
        {
            saveSystem.Delete();
        }

        ShowStartPresetPage();
    }

    public void OnClickBackFromConfirm()
    {
        ShowMainPage();
    }

    private void StartNewGameAndEnterGameScene(string presetId)
    {
        if (AppRoot.Instance == null || AppRoot.Instance.SceneLoader == null)
        {
            SetFeedback("新开失败：AppRoot 未就绪。");
            return;
        }

        AppRoot.Instance.SceneLoader.StartNewGame(gameSceneName, presetId);
    }

    private void RefreshContinueState()
    {
        SaveSystem saveSystem = AppRoot.Instance != null ? AppRoot.Instance.SaveSystem : null;
        bool hasSave = saveSystem != null && saveSystem.HasSave();
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(hasSave);
            continueButton.interactable = hasSave;
        }
    }

    private void ShowMainPage()
    {
        MainMenuUIManager uiManager = GetMainMenuUiManagerOrLog("ShowMainPage");
        if (uiManager != null)
        {
            uiManager.OpenPage(MainMenuPageKey.MainPage);
        }
    }

    private void ShowConfirmPage()
    {
        MainMenuUIManager uiManager = GetMainMenuUiManagerOrLog("ShowConfirmPage");
        if (uiManager != null)
        {
            uiManager.OpenPage(MainMenuPageKey.ConfirmPage);
        }
    }

    private void ShowStartPresetPage()
    {
        if (!TryBuildStartPresetButtons())
        {
            ShowMainPage();
            SetFeedback(PRESET_LOAD_FAILED_TEXT);
            return;
        }

        MainMenuUIManager uiManager = GetMainMenuUiManagerOrLog("ShowStartPresetPage");
        if (uiManager != null)
        {
            uiManager.OpenPage(MainMenuPageKey.StartPresetPage);
        }
    }

    private bool TryBuildStartPresetButtons()
    {
        ClearGeneratedPresetButtons();
        if (AppRoot.Instance == null || AppRoot.Instance.Configs == null)
        {
            return false;
        }

        IReadOnlyList<StartGameConfig> presets = AppRoot.Instance.Configs.GetStartGameConfigs();
        if (presets == null || presets.Count == 0)
        {
            return false;
        }

        if (startPresetButtonContainer == null || startPresetButtonPrefab == null)
        {
            return false;
        }

        for (int i = 0; i < presets.Count; i++)
        {
            StartGameConfig preset = presets[i];
            if (preset == null || preset.PlayerTemplate == null || string.IsNullOrWhiteSpace(preset.PresetId))
            {
                continue;
            }

            Button button = Instantiate(startPresetButtonPrefab, startPresetButtonContainer);
            _generatedPresetButtons.Add(button);
            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = string.IsNullOrWhiteSpace(preset.Description) ? preset.PresetId : preset.Description;
            }

            string capturedPresetId = preset.PresetId;
            button.onClick.AddListener(() => StartNewGameAndEnterGameScene(capturedPresetId));
        }

        return _generatedPresetButtons.Count > 0;
    }

    private void ClearGeneratedPresetButtons()
    {
        for (int i = 0; i < _generatedPresetButtons.Count; i++)
        {
            Button button = _generatedPresetButtons[i];
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }

        _generatedPresetButtons.Clear();
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private static MainMenuUIManager GetMainMenuUiManagerOrLog(string caller)
    {
        if (MainMenuUIManager.Instance == null)
        {
            Debug.LogError($"MainMenuController.{caller} -> MainMenuUIManager.Instance 为空。");
            return null;
        }

        return MainMenuUIManager.Instance;
    }

}

