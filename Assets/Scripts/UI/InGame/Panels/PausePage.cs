using TMPro;
using UI.InGame.Pause;
using UnityEngine;

public sealed class PausePage : MonoBehaviour
{
    [Header("Logic")]
    [SerializeField] private PauseActions actions;

    [Header("UI")]
    [SerializeField] private GameObject saveFailedDialogRoot;
    [SerializeField] private TMP_Text saveFailedMessageText;

    public void Show()
    {
        SetSaveFailedDialogVisible(false);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        SetSaveFailedDialogVisible(false);
        gameObject.SetActive(false);
    }

    public void OnClose()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseUIEntry(UIKey.PausePage);
        }
    }

    public void OnSaveAndBackToMainMenu()
    {
        SetSaveFailedDialogVisible(false);
        if (actions == null)
        {
            SetSaveFailedMessage("保存失败：PauseActions 未绑定。");
            SetSaveFailedDialogVisible(true);
            return;
        }

        actions.TrySaveThenBackToMenu((msg) =>
        {
            SetSaveFailedMessage(msg);
            SetSaveFailedDialogVisible(true);
        });
    }

    public void OnForceBackToMainMenu()
    {
        if (actions != null)
        {
            actions.ForceBackToMenu();
        }
    }

    public void OnCancelSaveFailure()
    {
        SetSaveFailedDialogVisible(false);
    }

    private void SetSaveFailedDialogVisible(bool visible)
    {
        if (saveFailedDialogRoot != null)
        {
            saveFailedDialogRoot.SetActive(visible);
        }
    }

    private void SetSaveFailedMessage(string message)
    {
        if (saveFailedMessageText != null)
        {
            saveFailedMessageText.text = message;
        }
    }
}

