using UnityEngine;

public class HudStatusPanel : MonoBehaviour
{
    [SerializeField] private PlayerDatePanel playerStatusPanel;

    [SerializeField] private RouteInfoPanel routeStatusPanel;

    private void OnValidate()
    {
        if (playerStatusPanel == null || routeStatusPanel == null)
        {
            Debug.LogWarning(
                "HudStatusPanel 未完成 Inspector 绑定：请手动拖拽 playerStatusPanel 与 routeStatusPanel。",
                this
            );
        }
    }

    public void RefreshStatus()
    {
        if (playerStatusPanel != null)
        {
            playerStatusPanel.UpdateInfo();
        }

        if (routeStatusPanel != null)
        {
            routeStatusPanel.UpdateInfo();
        }
    }

    public void UpdateInfo()
    {
        // Legacy transition entry. Prefer RefreshStatus().
        RefreshStatus();
    }

    public void ShowStatus()
    {
        gameObject.SetActive(true);
        if (playerStatusPanel != null)
        {
            playerStatusPanel.Show();
        }

        if (routeStatusPanel != null)
        {
            routeStatusPanel.Show();
        }
    }

    public void Show()
    {
        // Legacy transition entry. Prefer ShowStatus().
        ShowStatus();
    }

    public void HideStatus()
    {
        if (playerStatusPanel != null)
        {
            playerStatusPanel.Hide();
        }

        if (routeStatusPanel != null)
        {
            routeStatusPanel.Hide();
        }

        gameObject.SetActive(false);
    }

    public void Hide()
    {
        // Legacy transition entry. Prefer HideStatus().
        HideStatus();
    }

    public void SetPlayerStatusVisible(bool visible)
    {
        if (playerStatusPanel == null)
        {
            return;
        }

        if (visible)
        {
            playerStatusPanel.Show();
        }
        else
        {
            playerStatusPanel.Hide();
        }
    }

    public bool IsPlayerStatusVisible()
    {
        return playerStatusPanel != null && playerStatusPanel.gameObject.activeSelf;
    }

    public void RefreshPlayerStatus()
    {
        if (playerStatusPanel == null)
        {
            return;
        }

        playerStatusPanel.UpdateInfo();
    }

    public void ToggleStatsPanel()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("HudStatusPanel.ToggleStatsPanel -> UIManager.Instance 为空。");
            return;
        }

        bool isCombatStatsOpen = UIManager.Instance.IsCombatStatsOpen();
        if (isCombatStatsOpen)
        {
            UIManager.Instance.SetCombatStatsOpen(false);
            SetPlayerStatusVisible(true);
            return;
        }

        SetPlayerStatusVisible(false);
        UIManager.Instance.SetCombatStatsOpen(true);
    }

    public void EnsureCombatStatsVisibleIfPlayerStatsVisible()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("HudStatusPanel.EnsureCombatStatsVisibleIfPlayerStatsVisible -> UIManager.Instance 为空。");
            return;
        }

        bool playerVisible = playerStatusPanel != null && playerStatusPanel.gameObject.activeSelf;
        if (!playerVisible)
        {
            return;
        }

        SetPlayerStatusVisible(false);
        UIManager.Instance.SetCombatStatsOpen(true);
    }

    public void OnOpenCombatStats()
    {
        // Legacy transition entry. Prefer ToggleStatsPanel().
        ToggleStatsPanel();
    }
}

