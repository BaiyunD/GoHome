using System.Collections.Generic;
using UnityEngine;

public enum MainMenuPageKey
{
    None,
    MainPage,
    ConfirmPage,
    StartPresetPage
}

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance { get; private set; }

    [SerializeField] private GameObject mainPageRoot;
    [SerializeField] private GameObject confirmPageRoot;
    [SerializeField] private GameObject startPresetPageRoot;

    private readonly Stack<MainMenuPageKey> _pageStack = new Stack<MainMenuPageKey>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public MainMenuPageKey GetCurrentPage()
    {
        return _pageStack.Count > 0 ? _pageStack.Peek() : MainMenuPageKey.None;
    }

    public void OpenPage(MainMenuPageKey key)
    {
        if (key == MainMenuPageKey.None)
        {
            return;
        }

        MainMenuPageKey current = GetCurrentPage();
        if (current == key)
        {
            SetPageVisible(key, true);
            return;
        }

        if (current != MainMenuPageKey.None)
        {
            SetPageVisible(current, false);
        }

        _pageStack.Push(key);
        SetPageVisible(key, true);
        Debug.Log($"开启【主界面页面】{key}");
    }

    public void CloseTopPage()
    {
        if (_pageStack.Count == 0)
        {
            return;
        }

        MainMenuPageKey closing = _pageStack.Pop();
        SetPageVisible(closing, false);
        Debug.Log($"关闭【主界面页面】{closing}");

        MainMenuPageKey revealed = GetCurrentPage();
        if (revealed != MainMenuPageKey.None)
        {
            SetPageVisible(revealed, true);
        }
    }

    public void CloseAllPagesByStackOrder()
    {
        while (_pageStack.Count > 0)
        {
            CloseTopPage();
        }

        SetPageVisible(MainMenuPageKey.MainPage, false);
        SetPageVisible(MainMenuPageKey.ConfirmPage, false);
        SetPageVisible(MainMenuPageKey.StartPresetPage, false);
    }

    public void ResetToDefaultPage()
    {
        CloseAllPagesByStackOrder();
        OpenPage(MainMenuPageKey.MainPage);
    }

    private void SetPageVisible(MainMenuPageKey key, bool visible)
    {
        GameObject root = ResolveRoot(key);
        if (root != null)
        {
            root.SetActive(visible);
        }
    }

    private GameObject ResolveRoot(MainMenuPageKey key)
    {
        switch (key)
        {
            case MainMenuPageKey.MainPage:
                return mainPageRoot;
            case MainMenuPageKey.ConfirmPage:
                return confirmPageRoot;
            case MainMenuPageKey.StartPresetPage:
                return startPresetPageRoot;
            default:
                return null;
        }
    }

}

