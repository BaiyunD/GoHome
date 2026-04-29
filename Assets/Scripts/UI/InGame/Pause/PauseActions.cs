using System;
using UnityEngine;

namespace UI.InGame.Pause
{
    public sealed class PauseActions : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        public bool TrySave(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (GameManager.Instance == null)
            {
                errorMessage = "保存失败：GameManager 未就绪。";
                return false;
            }

            if (!GameManager.Instance.TryBuildSaveData(out SaveData saveData, out string buildError))
            {
                errorMessage = $"保存失败：{buildError}";
                return false;
            }

            try
            {
                if (AppRoot.Instance == null || AppRoot.Instance.SaveSystem == null)
                {
                    errorMessage = "保存失败：SaveSystem 未就绪。";
                    return false;
                }

                AppRoot.Instance.SaveSystem.Save(saveData);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"保存失败：{ex.Message}";
                return false;
            }
        }

        public void BackToMainMenu()
        {
            if (AppRoot.Instance != null && AppRoot.Instance.SceneLoader != null)
            {
                AppRoot.Instance.SceneLoader.LoadMainMenu(mainMenuSceneName);
            }
        }

        public void ForceBackToMenu()
        {
            BackToMainMenu();
        }

        public void TrySaveThenBackToMenu(Action<string> onFailedMessage)
        {
            if (TrySave(out string error))
            {
                BackToMainMenu();
                return;
            }

            onFailedMessage?.Invoke(string.IsNullOrWhiteSpace(error) ? "保存失败：未知错误。" : error);
        }
    }
}

