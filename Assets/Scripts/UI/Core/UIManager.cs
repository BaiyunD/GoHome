using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get; private set;
    }

    [SerializeField] private HudStatusPanel hudStatusPanel;         //HUD状态面板（兼容玩家数据+路程信息）
    [SerializeField, FormerlySerializedAs("actionButtons")] private ActionBarPanel actionBarPanel;           //游戏中按钮
    [SerializeField, FormerlySerializedAs("eventNarrationPanel")] private EventNarrationModal eventNarrationModal; //事件叙述文本（在 UIManager Inspector 绑定）
    [SerializeField] private ResultToastModal resultToastModal; // 通用提示弹窗（可选绑定）
    [SerializeField] private RandomEventPanel randomEventPanel;     //随机事件面板
    [SerializeField, FormerlySerializedAs("inventoryPanel")] private InventoryPage inventoryPage;         //背包面板
    [SerializeField, FormerlySerializedAs("craftPanel")] private CraftPage craftPage;         //背包面板
    [SerializeField] private ShopPage shopPage;                  // 商店页面
    [SerializeField] private BattlePanel battlePanel;               //战斗面板
    [SerializeField, FormerlySerializedAs("restSummaryPanel")] private RestPage restPage;     //休息结算面板
    [SerializeField, FormerlySerializedAs("endPanel")] private EndPage endPage;                     //结束面板
    [SerializeField, FormerlySerializedAs("traitPanel")] private TraitsPage traitsPage;                 //特性面板
    [SerializeField] private CombatStatsPanel combatStatsPanel;     //战斗属性面板
    [SerializeField] private PausePage pausePage;                   //暂停页面（Page）
    [SerializeField] private bool enablePhase2PageMappingInDevOnly = true; // 第二阶段映射（仅开发模式）
    [SerializeField] private bool enableShopPageInDevOnly = true; // 商店入口开关（仅开发模式）
    //TODO:每个模块单独背景

    private List<UIKey> _persistentPanelTypes = new List<UIKey>(); // 已打开的常驻面板
    private Stack<UIKey> _pageStack = new Stack<UIKey>();      // 页面栈（支持嵌套）
    private UIStateMachine _uiStateMachine;
    private UIRouter _uiRouter;
    private bool _allowRestOpenFromFlow;
    private bool _allowEndOpenFromFlow;
    private string _pendingResultToastMessage;

    private static readonly HashSet<UIKey> AutoCloseNarrationOnOpen = new HashSet<UIKey>()
    {
        UIKey.InventoryPage,
        UIKey.CraftPage,
        UIKey.ShopPage,
        UIKey.TraitsPage,
        UIKey.PausePage,
        UIKey.RestPage,
        UIKey.EndPage,
        UIKey.RandomEvent,
        UIKey.ShopPage,
        UIKey.Battle
    };

    private void Awake()
    {
        Instance = this;
        if (hudStatusPanel == null)
        {
            Debug.LogWarning("UIManager.hudStatusPanel 未绑定，请在 Inspector 手动拖拽。", this);
        }
        _uiStateMachine = new UIStateMachine(_persistentPanelTypes, _pageStack, ResolveLayerByPanelType);
        _uiRouter = new UIRouter(
            _uiStateMachine,
            ShowPanel,
            HidePanel,
            SetPermanentPanelsVisible,
            HideEventNarrationText,
            IsPersistentPanel,
            CanOpenPage
        );
        ResetUIVisibilityForBootstrap();
    }

    private void Start()
    {
        // Keep Start side-effect free to avoid hiding panels
        // after NewGameEvent has already opened HUD + ActionBar.
    }

    private void OnDestroy()
    {
        UnbindGameManagerEvents();
    }

    private void OnEnable()
    {
        BindGameManagerEvents();
    }

    private void OnDisable()
    {
        UnbindGameManagerEvents();
    }

    private void NewGameAction()
    {
        Debug.Log("开启新游戏面板");
        foreach (var panel in _persistentPanelTypes.ToList())
        {
            CloseUIEntry(panel);
        }
        foreach (var panel in _pageStack.ToList())
        {
            CloseUIEntry(panel);
        }
        OpenUIEntry(UIKey.HudStatus);
        OpenUIEntry(UIKey.ActionBar);
    }

    private void BindGameManagerEvents()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.NewGameEvent -= NewGameAction;
        GameManager.Instance.NewGameEvent += NewGameAction;
    }

    private void UnbindGameManagerEvents()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.NewGameEvent -= NewGameAction;
    }

    public void UpdateInfo()
    {
        if (hudStatusPanel != null)
        {
            hudStatusPanel.RefreshStatus();
        }
        if (combatStatsPanel != null && combatStatsPanel.gameObject.activeInHierarchy)
        {
            combatStatsPanel.RefreshCombatStats();
        }
        if (traitsPage != null)
        {
            traitsPage.Refresh();
        }
    }

    public void SetHudPlayerStatusVisible(bool visible)
    {
        if (hudStatusPanel == null)
        {
            Debug.LogWarning("UIManager.SetHudPlayerStatusVisible -> hudStatusPanel 未绑定");
            return;
        }

        hudStatusPanel.SetPlayerStatusVisible(visible);
    }

    public bool IsCombatStatsOpen()
    {
        if (!EnsureCombatStatsPanelReference())
        {
            return false;
        }

        return combatStatsPanel != null && combatStatsPanel.gameObject.activeSelf;
    }

    public void SetCombatStatsOpen(bool open)
    {
        if (open)
        {
            OpenUIEntry(UIKey.CombatStats);
            return;
        }

        CloseUIEntry(UIKey.CombatStats);
    }

    public void EnsureCombatStatsVisibleIfPlayerStatsVisible()
    {
        if (hudStatusPanel == null)
        {
            Debug.LogWarning("UIManager.EnsureCombatStatsVisibleIfPlayerStatsVisible -> hudStatusPanel 未绑定");
            return;
        }

        hudStatusPanel.EnsureCombatStatsVisibleIfPlayerStatsVisible();
    }

    public void RefreshActiveStatsPanelInHud()
    {
        if (IsCombatStatsOpen())
        {
            if (combatStatsPanel != null)
            {
                combatStatsPanel.RefreshCombatStats();
            }
            return;
        }

        if (hudStatusPanel != null && hudStatusPanel.IsPlayerStatusVisible())
        {
            hudStatusPanel.RefreshPlayerStatus();
        }
    }

    public void OpenUIEntry(UIKey type)
    {
        OpenPanelCore(type);
    }

    public void CloseUIEntry(UIKey type)
    {
        ClosePanelCore(type);
    }

    public void CloseAllGameUiImmediate()
    {
        CloseActiveModals();
        ClosePagesByStackOrder();
        CloseActiveNonPagePanels();
        CheckUnclosedUiStateAndWarn();
    }

    public void ShowEventNarrationText(string text)
    {
        if (eventNarrationModal == null)
        {
            Debug.LogWarning("UIManager.ShowEventNarrationText -> eventNarrationModal 未绑定");
            return;
        }

        ShowEventNarrationModal(text);
    }

    public void HideEventNarrationText()
    {
        if (eventNarrationModal == null)
        {
            Debug.LogWarning("UIManager.HideEventNarrationText -> eventNarrationModal 未绑定");
            return;
        }

        HideEventNarrationModal();
    }

    public void ShowRandomEventOptions(List<EventOption> options, System.Action<int> onSelected)
    {
        if (randomEventPanel == null) return;
        randomEventPanel.ShowOptions(options, onSelected);
    }

    public void HideRandomEventOptions()
    {
        if (randomEventPanel == null) return;
        randomEventPanel.HideOptions();
    }

    public void ShowRestSummary(RestSettlement settlement)
    {
        OpenRestPanelFromFlow();
        if (restPage != null)
        {
            restPage.Show(settlement);
        }
    }

    public void OpenEndPanelFromFlow()
    {
        // Flow-only page gate: End page must be opened via flow method, not generic open entry.
        _allowEndOpenFromFlow = true;
        OpenUIEntry(UIKey.EndPage);
    }

    public void OpenRestPanelFromFlow()
    {
        // Flow-only page gate: Rest page must be opened via flow method, not generic open entry.
        _allowRestOpenFromFlow = true;
        OpenUIEntry(UIKey.RestPage);
    }

    private void ShowPanel(UIKey panelType)
    {
        switch (panelType)
        {
            case UIKey.None:
                break;
            case UIKey.HudStatus:
                if (hudStatusPanel != null)
                {
                    hudStatusPanel.Show();
                }
                break;
            case UIKey.ActionBar:
                actionBarPanel.Show();
                break;
            case UIKey.RandomEvent:
                randomEventPanel.Show();
                break;
            case UIKey.InventoryPage:
                inventoryPage.Show();
                break;
            case UIKey.CraftPage:
                craftPage.Show();
                break;
            case UIKey.ShopPage:
                if (shopPage != null)
                {
                    shopPage.Show();
                }
                break;
            case UIKey.Battle:
                battlePanel.Show();
                break;
            case UIKey.RestPage:
                restPage.Show();
                break;
            case UIKey.EndPage:
                endPage.Show();
                break;
            case UIKey.TraitsPage:
                if (traitsPage != null)
                {
                    traitsPage.Show();
                }
                break;
            case UIKey.CombatStats:
                if (EnsureCombatStatsPanelReference())
                {
                    combatStatsPanel.Show();
                }
                else
                {
                    Debug.LogError("UIManager.ShowPanel -> CombatStatsPanel 未绑定且自动查找失败。");
                }
                break;
            case UIKey.PausePage:
                if (EnsurePausePageReference())
                {
                    pausePage.Show();
                }
                else
                {
                    Debug.LogWarning("UIManager.ShowPanel -> PausePage 未绑定且自动查找失败。");
                }
                break;
            case UIKey.EventNarrationModal:
                if (eventNarrationModal != null)
                {
                    eventNarrationModal.Show();
                }
                break;
            case UIKey.ResultToastModal:
                if (EnsureResultToastModalReference())
                {
                    resultToastModal.Show(string.IsNullOrWhiteSpace(_pendingResultToastMessage) ? string.Empty : _pendingResultToastMessage);
                    _pendingResultToastMessage = null;
                }
                else
                {
                    Debug.LogWarning("UIManager.ShowPanel -> ResultToastModal 未绑定且自动查找失败。");
                }
                break;
        }
    }

    private void HidePanel(UIKey uiKey)
    {
        switch (uiKey)
        {
            case UIKey.None:
                break;
            case UIKey.HudStatus:
                if (hudStatusPanel != null)
                {
                    hudStatusPanel.Hide();
                }
                break;
            case UIKey.ActionBar:
                actionBarPanel.Hide();
                break;
            case UIKey.RandomEvent:
                randomEventPanel.Hide();
                break;
            case UIKey.InventoryPage:
                inventoryPage.Hide();
                break;
            case UIKey.CraftPage:
                craftPage.Hide();
                break;
            case UIKey.ShopPage:
                if (shopPage != null)
                {
                    shopPage.Hide();
                }
                break;
            case UIKey.Battle:
                battlePanel.Hide();
                break;
            case UIKey.RestPage:
                restPage.Hide();
                break;
            case UIKey.EndPage:
                endPage.Hide();
                break;
            case UIKey.TraitsPage:
                if (traitsPage != null)
                {
                    traitsPage.Hide();
                }
                break;
            case UIKey.CombatStats:
                if (EnsureCombatStatsPanelReference())
                {
                    combatStatsPanel.Hide();
                }
                break;
            case UIKey.PausePage:
                if (EnsurePausePageReference())
                {
                    pausePage.Hide();
                }
                break;
            case UIKey.EventNarrationModal:
                if (eventNarrationModal != null)
                {
                    eventNarrationModal.Hide();
                }
                break;
            case UIKey.ResultToastModal:
                if (EnsureResultToastModalReference())
                {
                    resultToastModal.HideImmediate();
                }
                break;
        }
    }

    private bool IsPersistentPanel(UIKey uiKey)
    {
        return uiKey == UIKey.HudStatus ||
               uiKey == UIKey.ActionBar;
    }

    private void AutoCloseCombatStatsOnPageSwitch(UIKey targetPanelType)
    {
        if (targetPanelType == UIKey.CombatStats)
        {
            return;
        }

        if (!IsPhase2PageMappingEnabled())
        {
            return;
        }

        if (!UIPageMapping.IsPhase2TargetPage(targetPanelType))
        {
            return;
        }

        if (combatStatsPanel == null || !combatStatsPanel.gameObject.activeSelf)
        {
            return;
        }

        CloseUIEntry(UIKey.CombatStats);
    }

    private UIStateLayer ResolveLayerByPanelType(UIKey uiKey)
    {
        if (uiKey == UIKey.None)
        {
            return UIStateLayer.Panel;
        }

        if (uiKey == UIKey.PausePage)
        {
            return UIStateLayer.Page;
        }

        if (uiKey == UIKey.EventNarrationModal || uiKey == UIKey.ResultToastModal)
        {
            return UIStateLayer.Modal;
        }

        if (IsPhase2PageMappingEnabled() && UIPageMapping.IsPhase2TargetPage(uiKey))
        {
            return UIStateLayer.Page;
        }

        return UIStateLayer.Panel;
    }

    private void ResetUIVisibilityForBootstrap()
    {
        HidePanel(UIKey.HudStatus);
        HidePanel(UIKey.ActionBar);
        HidePanel(UIKey.RandomEvent);
        HidePanel(UIKey.InventoryPage);
        HidePanel(UIKey.CraftPage);
        HidePanel(UIKey.ShopPage);
        HidePanel(UIKey.Battle);
        HidePanel(UIKey.CombatStats);
        HidePanel(UIKey.RestPage);
        HidePanel(UIKey.EndPage);
        HidePanel(UIKey.TraitsPage);

        _allowRestOpenFromFlow = false;
        _allowEndOpenFromFlow = false;
        _uiStateMachine.ResetRuntimeState();
    }

    private void ClosePagesByStackOrder()
    {
        while (true)
        {
            UIKey topPage = _uiStateMachine.PeekTopPage();
            if (topPage == UIKey.None)
            {
                break;
            }

            CloseUIEntry(topPage);
        }
    }

    private void CloseActiveModals()
    {
        while (true)
        {
            IReadOnlyList<UIKey> activeModals = _uiStateMachine.GetActiveModalsSnapshot();
            if (activeModals == null || activeModals.Count == 0)
            {
                break;
            }

            // No ordering semantics for active modals. Close any one per iteration.
            CloseUIEntry(activeModals[0]);
        }
    }

    private void CloseActiveNonPagePanels()
    {
        IReadOnlyList<UIKey> activePanels = _uiStateMachine.GetActivePanelsSnapshot();
        for (int i = 0; i < activePanels.Count; i++)
        {
            UIKey panel = activePanels[i];
            if (panel == UIKey.None)
            {
                continue;
            }

            UIStateLayer layer = _uiStateMachine.ResolveLayer(panel);
            if (layer == UIStateLayer.Page || layer == UIStateLayer.Modal)
            {
                continue;
            }

            CloseUIEntry(panel);
        }
    }

    private void CheckUnclosedUiStateAndWarn()
    {
        UIStateSnapshot snapshot = _uiStateMachine.CreateSnapshot();
        if (snapshot.PageStack != null && snapshot.PageStack.Count > 0)
        {
            Debug.LogWarning($"当前未关闭：【页面】{string.Join(", ", snapshot.PageStack)}");
        }

        List<UIKey> unclosedPanels = new List<UIKey>();
        if (snapshot.ActivePanels != null)
        {
            for (int i = 0; i < snapshot.ActivePanels.Count; i++)
            {
                UIKey panel = snapshot.ActivePanels[i];
                UIStateLayer layer = _uiStateMachine.ResolveLayer(panel);
                if (layer == UIStateLayer.Page || layer == UIStateLayer.Modal)
                {
                    continue;
                }

                unclosedPanels.Add(panel);
            }
        }
        if (unclosedPanels.Count > 0)
        {
            Debug.LogWarning($"当前未关闭：【面板】{string.Join(", ", unclosedPanels)}");
        }

        if (snapshot.ModalStack != null && snapshot.ModalStack.Count > 0)
        {
            Debug.LogWarning($"当前未关闭：【modal】{string.Join(", ", snapshot.ModalStack)}");
        }
    }

    private void SetPermanentPanelsVisible(bool visible)
    {
        foreach (var pt in _persistentPanelTypes)
        {
            if (visible)
            {
                ShowPanel(pt);
            }
            else
            {
                HidePanel(pt);
            }
        }
    }

    public UIStateSnapshot GetStateSnapshot()
    {
        return _uiStateMachine.CreateSnapshot();
    }

    private bool IsPhase2PageMappingEnabled()
    {
        if (!enablePhase2PageMappingInDevOnly)
        {
            return false;
        }

        return Debug.isDebugBuild;
    }

    private bool CanOpenPage(UIKey panelType)
    {
        if (panelType == UIKey.ShopPage)
        {
            if (!IsShopPageEnabled())
            {
                return false;
            }

            if (ShopService.Instance != null && !ShopService.Instance.IsFeatureEnabled)
            {
                return false;
            }
        }

        if (!IsPhase2PageMappingEnabled())
        {
            return true;
        }

        if (!UIPageMapping.IsFlowOnlyPage(panelType))
        {
            return true;
        }

        // Rest/End are flow-only pages. They are allowed only after explicit flow gate methods set flags.
        if (panelType == UIKey.RestPage && _allowRestOpenFromFlow)
        {
            _allowRestOpenFromFlow = false;
            return true;
        }

        if (panelType == UIKey.EndPage && _allowEndOpenFromFlow)
        {
            _allowEndOpenFromFlow = false;
            return true;
        }

        return false;
    }

    private bool IsShopPageEnabled()
    {
        if (!enableShopPageInDevOnly)
        {
            return false;
        }

        return Debug.isDebugBuild;
    }

    private void OpenPanelCore(UIKey type)
    {
        AutoCloseCombatStatsOnPageSwitch(type);
        if (AutoCloseNarrationOnOpen.Contains(type))
        {
            if (IsModalOpen(UIKey.EventNarrationModal))
            {
                HideEventNarrationModal();
            }
        }
        _uiRouter.RouteOpenPanel(type);
        LogUiOpen(type);
    }

    private void ClosePanelCore(UIKey type)
    {
        _uiRouter.RouteClosePanel(type);
        LogUiClose(type);
    }

    private void LogUiOpen(UIKey type)
    {
        Debug.LogFormat("开启【{0}】{1}", FormatLayer(_uiStateMachine.ResolveLayer(type)), type);
    }

    private void LogUiClose(UIKey type)
    {
        Debug.LogFormat("关闭【{0}】{1}", FormatLayer(_uiStateMachine.ResolveLayer(type)), type);
    }

    private static string FormatLayer(UIStateLayer layer)
    {
        switch (layer)
        {
            case UIStateLayer.Page:
                return "页面";
            case UIStateLayer.Modal:
                return "modal";
            default:
                return "面板";
        }
    }

    public void ShowResultToast(string message)
    {
        _pendingResultToastMessage = message;
        if (EnsureResultToastModalReference() && resultToastModal.gameObject.activeInHierarchy)
        {
            resultToastModal.Show(string.IsNullOrWhiteSpace(_pendingResultToastMessage) ? string.Empty : _pendingResultToastMessage);
            _pendingResultToastMessage = null;
            return;
        }

        OpenUIEntry(UIKey.ResultToastModal);
    }

    public void HideResultToast()
    {
        CloseUIEntry(UIKey.ResultToastModal);
    }

    public void ShowEventNarrationModal(string message)
    {
        if (eventNarrationModal != null)
        {
            eventNarrationModal.SetText(message);
        }
        OpenUIEntry(UIKey.EventNarrationModal);
    }

    public void HideEventNarrationModal()
    {
        CloseUIEntry(UIKey.EventNarrationModal);
    }

    public void ClearEventNarrationModalText()
    {
        if (eventNarrationModal == null)
        {
            Debug.LogWarning("UIManager.ClearEventNarrationModalText -> eventNarrationModal 未绑定");
            return;
        }

        eventNarrationModal.ClearText();
    }

    public bool IsModalOpen(UIKey key)
    {
        if (_uiStateMachine == null)
        {
            return false;
        }

        if (_uiStateMachine.ResolveLayer(key) != UIStateLayer.Modal)
        {
            return false;
        }

        return _uiStateMachine.IsModalActive(key);
    }

    private bool EnsureCombatStatsPanelReference()
    {
        if (combatStatsPanel != null)
        {
            return true;
        }

        CombatStatsPanel[] candidates = FindObjectsOfType<CombatStatsPanel>(true);
        if (candidates != null && candidates.Length > 0)
        {
            combatStatsPanel = candidates[0];
            Debug.LogWarning("UIManager -> CombatStatsPanel 未在 Inspector 绑定，已自动回填引用。");
            return true;
        }

        return false;
    }

    private bool EnsureResultToastModalReference()
    {
        if (resultToastModal != null)
        {
            return true;
        }

        ResultToastModal[] candidates = FindObjectsOfType<ResultToastModal>(true);
        if (candidates != null && candidates.Length > 0)
        {
            resultToastModal = candidates[0];
            Debug.LogWarning("UIManager -> ResultToastModal 未在 Inspector 绑定，已自动回填引用。");
            return true;
        }

        return false;
    }

    private bool EnsurePausePageReference()
    {
        if (pausePage != null)
        {
            return true;
        }

        PausePage[] candidates = FindObjectsOfType<PausePage>(true);
        if (candidates != null && candidates.Length > 0)
        {
            pausePage = candidates[0];
            Debug.LogWarning("UIManager -> PausePage 未在 Inspector 绑定，已自动回填引用。");
            return true;
        }

        return false;
    }

}

public enum UIKey
{
    None,
    HudStatus,       // 常驻
    ActionBar,   // Panel
    RandomEvent,     // Panel
    InventoryPage,       // Page
    CraftPage,        // Page
    Battle,          // 非独占
    CombatStats,     // Panel（二级详情）
    RestPage,            // Page
    EndPage,             // Page
    TraitsPage,          // Page
    PausePage,           // Page
    ShopPage,            // Page
    EventNarrationModal, // modal
    ResultToastModal     // modal
}
