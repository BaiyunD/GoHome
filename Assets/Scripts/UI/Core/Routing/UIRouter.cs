using UnityEngine;

public class UIRouter
{
    private readonly UIStateMachine _stateMachine;
    private readonly System.Action<UIKey> _showPanel;
    private readonly System.Action<UIKey> _hidePanel;
    private readonly System.Action<bool> _setPersistentPanelsVisible;
    private readonly System.Action _hideEventNarration;
    private readonly System.Func<UIKey, bool> _isPersistentPanelByDefinition;
    private readonly System.Func<UIKey, bool> _canOpenPage;

    public UIRouter(
        UIStateMachine stateMachine,
        System.Action<UIKey> showPanel,
        System.Action<UIKey> hidePanel,
        System.Action<bool> setPersistentPanelsVisible,
        System.Action hideEventNarration,
        System.Func<UIKey, bool> isPersistentPanelByDefinition,
        System.Func<UIKey, bool> canOpenPage
    )
    {
        _stateMachine = stateMachine;
        _showPanel = showPanel;
        _hidePanel = hidePanel;
        _setPersistentPanelsVisible = setPersistentPanelsVisible;
        _hideEventNarration = hideEventNarration;
        _isPersistentPanelByDefinition = isPersistentPanelByDefinition;
        _canOpenPage = canOpenPage;
    }

    public void RouteOpenPanel(UIKey panelType)
    {
        UIStateLayer layer = _stateMachine.ResolveLayer(panelType);
        if (layer == UIStateLayer.Page)
        {
            OpenPage(panelType);
            return;
        }

        if (layer == UIStateLayer.Modal)
        {
            OpenModal(panelType);
            return;
        }

        OpenPanel(panelType);
    }

    public void RouteClosePanel(UIKey uiKey)
    {
        UIStateLayer layer = _stateMachine.ResolveLayer(uiKey);
        if (layer == UIStateLayer.Page)
        {
            ClosePage(uiKey);
            return;
        }

        if (layer == UIStateLayer.Modal)
        {
            CloseModal(uiKey);
            return;
        }

        ClosePanel(uiKey);
    }

    private void OpenPage(UIKey panelType)
    {
        if (!_canOpenPage(panelType))
        {
            Debug.LogWarning($"页面开启被拦截：{panelType}");
            return;
        }

        if (!_stateMachine.TryPushPage(panelType, out UIKey previousPage))
        {
            _showPanel(panelType);
            Debug.LogFormat("开启{0}面板（已在页面栈顶）", panelType);
            return;
        }

        if (previousPage == UIKey.None)
        {
            _setPersistentPanelsVisible(false);
        }
        else
        {
            _hidePanel(previousPage);
        }

        _showPanel(panelType);
        _stateMachine.MarkPanelOpened(panelType);
    }

    private void ClosePage(UIKey panelType)
    {
        if (_stateMachine.TryPopPage(panelType, out UIKey revealedPage))
        {
            _hidePanel(panelType);
            _stateMachine.MarkPanelClosed(panelType);
            if (revealedPage == UIKey.None)
            {
                _setPersistentPanelsVisible(true);
            }
            else
            {
                _showPanel(revealedPage);
            }
            return;
        }

        _hidePanel(panelType);
        _stateMachine.MarkPanelClosed(panelType);
        Debug.LogWarning($"尝试关闭不在页面栈顶的页面：{panelType}");
    }

    private void OpenModal(UIKey modalType)
    {
        if (_stateMachine.IsModalActive(modalType))
        {
            return;
        }

        _showPanel(modalType);
        _stateMachine.MarkModalOpened(modalType);
    }

    private void CloseModal(UIKey modalType)
    {
        if (!_stateMachine.IsModalActive(modalType))
        {
            return;
        }

        _hidePanel(modalType);
        _stateMachine.MarkModalClosed(modalType);
    }

    private void OpenPanel(UIKey panelType)
    {
        if (_isPersistentPanelByDefinition(panelType))
        {
            _stateMachine.MarkPersistentPanelOpened(panelType);
            _showPanel(panelType);
            return;
        }

        if (_stateMachine.IsPersistentPanel(panelType))
        {
            _showPanel(panelType);
            return;
        }

        _showPanel(panelType);
        _stateMachine.MarkPanelOpened(panelType);
    }

    private void ClosePanel(UIKey panelType)
    {
        _hidePanel(panelType);
        _stateMachine.MarkPanelClosed(panelType);
    }
}
