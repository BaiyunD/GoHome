using System.Collections.Generic;
using System.Linq;

public class UIStateMachine
{
    private readonly List<UIKey> _persistentPanels;
    private readonly Stack<UIKey> _pageStack;
    private readonly HashSet<UIKey> _activePanels;
    private readonly HashSet<UIKey> _activeModals;
    private readonly HashSet<UIKey> _persistentPanelSet;
    private readonly System.Func<UIKey, UIStateLayer> _layerResolver;

    public UIStateMachine(
        List<UIKey> persistentPanels,
        Stack<UIKey> pageStack,
        System.Func<UIKey, UIStateLayer> layerResolver
    )
    {
        _persistentPanels = persistentPanels;
        _pageStack = pageStack;
        _layerResolver = layerResolver;
        _activePanels = new HashSet<UIKey>();
        _activeModals = new HashSet<UIKey>();
        _persistentPanelSet = new HashSet<UIKey>();
    }

    public UIStateLayer ResolveLayer(UIKey panelType)
    {
        return _layerResolver(panelType);
    }

    public bool IsPersistentPanel(UIKey panelType)
    {
        return _persistentPanelSet.Contains(panelType);
    }

    public void MarkPanelOpened(UIKey panelType)
    {
        _activePanels.Add(panelType);
    }

    public void MarkPanelClosed(UIKey panelType)
    {
        _activePanels.Remove(panelType);
        if (_persistentPanelSet.Remove(panelType))
        {
            _persistentPanels.Remove(panelType);
        }
    }

    public void MarkModalOpened(UIKey modalType)
    {
        _activeModals.Add(modalType);
        _activePanels.Add(modalType);
    }

    public void MarkModalClosed(UIKey modalType)
    {
        _activeModals.Remove(modalType);
        _activePanels.Remove(modalType);
    }

    public IReadOnlyList<UIKey> GetActiveModalsSnapshot()
    {
        return _activeModals.ToList();
    }

    public bool IsModalActive(UIKey modalType)
    {
        return _activeModals.Contains(modalType);
    }

    public void MarkPersistentPanelOpened(UIKey panelType)
    {
        if (_persistentPanelSet.Add(panelType))
        {
            _persistentPanels.Add(panelType);
        }
        _activePanels.Add(panelType);
    }

    public bool TryPushPage(UIKey pageType, out UIKey previousPage)
    {
        previousPage = _pageStack.Count > 0 ? _pageStack.Peek() : UIKey.None;
        if (previousPage == pageType)
        {
            return false;
        }

        _pageStack.Push(pageType);
        return true;
    }

    public bool TryPopPage(UIKey pageType, out UIKey revealedPage)
    {
        revealedPage = UIKey.None;
        if (_pageStack.Count == 0)
        {
            return false;
        }

        if (_pageStack.Peek() == pageType)
        {
            _pageStack.Pop();
            revealedPage = _pageStack.Count > 0 ? _pageStack.Peek() : UIKey.None;
            return true;
        }

        UIKey[] stackSnapshot = _pageStack.ToArray();
        _pageStack.Clear();
        bool removed = false;

        for (int i = stackSnapshot.Length - 1; i >= 0; i--)
        {
            UIKey current = stackSnapshot[i];
            if (!removed && current == pageType)
            {
                removed = true;
                continue;
            }

            _pageStack.Push(current);
        }

        return false;
    }

    public UIStateSnapshot CreateSnapshot()
    {
        UIKey currentPage = _pageStack.Count > 0 ? _pageStack.Peek() : UIKey.None;
        List<UIKey> pageStackCopy = _pageStack.Reverse().ToList();
        List<UIKey> modalStackCopy = _activeModals.ToList();
        List<UIKey> activePanelsCopy = _activePanels.ToList();
        return new UIStateSnapshot(currentPage, activePanelsCopy, pageStackCopy, modalStackCopy);
    }

    public UIKey PeekTopPage()
    {
        return _pageStack.Count > 0 ? _pageStack.Peek() : UIKey.None;
    }

    public IReadOnlyList<UIKey> GetActivePanelsSnapshot()
    {
        return _activePanels.ToList();
    }

    public void ResetRuntimeState()
    {
        _persistentPanels.Clear();
        _persistentPanelSet.Clear();
        _pageStack.Clear();
        _activePanels.Clear();
        _activeModals.Clear();
    }
}
