using System.Collections.Generic;

public enum UIStateLayer
{
    Page,
    Panel,
    Modal
}

public struct UIStateEntry
{
    public UIKey UIKey;
    public UIStateLayer Layer;

    public UIStateEntry(UIKey uiKey, UIStateLayer layer)
    {
        UIKey = uiKey;
        Layer = layer;
    }
}

public struct UIStateSnapshot
{
    public UIKey CurrentPage;
    public IReadOnlyList<UIKey> ActivePanels;
    public IReadOnlyList<UIKey> PageStack;
    public IReadOnlyList<UIKey> ModalStack;

    public UIStateSnapshot(
        UIKey currentPage,
        IReadOnlyList<UIKey> activePanels,
        IReadOnlyList<UIKey> pageStack,
        IReadOnlyList<UIKey> modalStack
    )
    {
        CurrentPage = currentPage;
        ActivePanels = activePanels;
        PageStack = pageStack;
        ModalStack = modalStack;
    }
}

public static class UIPageMapping
{
    private static readonly HashSet<UIKey> Phase2TargetPages = new HashSet<UIKey>()
    {
        UIKey.InventoryPage,
        UIKey.CraftPage,
        UIKey.ShopPage,
        UIKey.TraitsPage,
        UIKey.PausePage,
        UIKey.RestPage,
        UIKey.EndPage
    };

    private static readonly HashSet<UIKey> FlowOnlyPages = new HashSet<UIKey>()
    {
        UIKey.RestPage,
        UIKey.EndPage
    };

    public static bool IsPhase2TargetPage(UIKey panelType)
    {
        return Phase2TargetPages.Contains(panelType);
    }

    public static bool IsFlowOnlyPage(UIKey panelType)
    {
        return FlowOnlyPages.Contains(panelType);
    }
}
