using UnityEngine;
using UnityEngine.UI;

public class ActionBarPanel : MonoBehaviour
{
    public static ActionBarPanel Instance
    {
        get; private set;
    }

    [SerializeField] private GameObject advance;
    [SerializeField] private GameObject rest;
    [SerializeField] private GameObject inventory;
    [SerializeField] private GameObject craft;
    [SerializeField] private GameObject trait;
    [SerializeField] private GameObject pause;
    [SerializeField] private GameObject shop;

    private ActionRegistry _registry;
    private ActionInvoker _invoker;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeActions();
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void OnAdvance()
    {
        InvokeAction(ActionId.Advance, "ActionBarPanel.OnAdvance");
    }

    public void OnExplore()
    {
        InvokeAction(ActionId.Explore, "ActionBarPanel.OnExplore");
    }

    public void OnRest()
    {
        InvokeAction(ActionId.Rest, "ActionBarPanel.OnRest");
    }

    public void OnInventoryOpen()
    {
        InvokeAction(ActionId.Inventory, "ActionBarPanel.OnInventoryOpen");
    }

    public void OnCraftOpen()
    {
        InvokeAction(ActionId.Craft, "ActionBarPanel.OnCraftOpen");
    }

    public void OnTraitsOpen()
    {
        InvokeAction(ActionId.Traits, "ActionBarPanel.OnTraitsOpen");
    }

    public void OnPauseOpen()
    {
        InvokeAction(ActionId.Pause, "ActionBarPanel.OnPauseOpen");
    }

    public void OnShopOpen()
    {
        InvokeAction(ActionId.Shop, "ActionBarPanel.OnShopOpen");
    }

    private void InitializeActions()
    {
        _registry = ActionBarBootstrap.BuildRegistry();
        ActionTelemetry telemetry = new ActionTelemetry();
        CooldownService cooldownService = new CooldownService();
        _invoker = new ActionInvoker(_registry, cooldownService, telemetry);
        RefreshActionBarState();
    }

    public void RefreshActionBarState()
    {
        if (_invoker == null)
        {
            return;
        }

        SetActionButtonState(ActionId.Advance, advance, "ActionBarPanel.Refresh.Advance");
        SetActionButtonState(ActionId.Rest, rest, "ActionBarPanel.Refresh.Rest");
        SetActionButtonState(ActionId.Inventory, inventory, "ActionBarPanel.Refresh.Inventory");
        SetActionButtonState(ActionId.Craft, craft, "ActionBarPanel.Refresh.Craft");
        SetActionButtonState(ActionId.Traits, trait, "ActionBarPanel.Refresh.Traits");
        SetActionButtonState(ActionId.Pause, pause, "ActionBarPanel.Refresh.Pause");
        SetActionButtonState(ActionId.Shop, shop, "ActionBarPanel.Refresh.Shop");
    }

    private void InvokeAction(ActionId actionId, string source)
    {
        if (_invoker == null)
        {
            return;
        }

        _invoker.Invoke(actionId, new ActionContext(this, source));
        RefreshActionBarState();
    }

    private void SetActionButtonState(ActionId actionId, GameObject buttonObject, string source)
    {
        if (buttonObject == null)
        {
            return;
        }

        bool canInvoke = _invoker.TryEvaluate(actionId, new ActionContext(this, source), out _);
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = canInvoke;
            return;
        }

        buttonObject.SetActive(canInvoke);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        RefreshActionBarState();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

