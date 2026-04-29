using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class RouteInfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text routeStatusText;

    private void Start()
    {
        GameManager.Instance.NewGameEvent += OnNewGame;
    }

    private void OnEnable()
    {
        BindCurrentRouteStats();
        BindRegionEvents();
        UpdateInfo();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NewGameEvent -= OnNewGame;
        }
    }

    private void OnNewGame()
    {
        BindCurrentRouteStats();
        UpdateInfo();
    }

    private void OnDisable()
    {
        UnsubscribeEvent();
        UnbindRegionEvents();
    }

    private void BindRegionEvents()
    {
        if (RouteProgressManager.Instance == null) return;
        RouteProgressManager.Instance.OnMainRegionChanged -= OnMainRegionChanged;
        RouteProgressManager.Instance.OnMainRegionChanged += OnMainRegionChanged;
    }

    private void UnbindRegionEvents()
    {
        if (RouteProgressManager.Instance == null) return;
        RouteProgressManager.Instance.OnMainRegionChanged -= OnMainRegionChanged;
    }

    private void OnMainRegionChanged(MainRegionData region)
    {
        UpdateLocation();
    }

    private void UnsubscribeEvent()
    {
        if (RouteProgressManager.Instance == null) return;
        RouteProgressManager.Instance.OnDistanceChanged -= OnDistanceChanged;
        RouteProgressManager.Instance.OnDayChanged -= OnDayChanged;
    }

    private void BindCurrentRouteStats()
    {
        UnsubscribeEvent();
        if (RouteProgressManager.Instance == null) return;
        RouteProgressManager.Instance.OnDistanceChanged += OnDistanceChanged;
        RouteProgressManager.Instance.OnDayChanged += OnDayChanged;
    }

    public void UpdateInfo()
    {
        RefreshRouteStatusText();
    }

    public void UpdateDay()
    {
        RefreshRouteStatusText();
    }

    public void UpdateDistance()
    {
        RefreshRouteStatusText();
    }

    public void UpdateLocation()
    {
        RefreshRouteStatusText();
    }

    private void RefreshRouteStatusText()
    {
        if (routeStatusText == null)
        {
            Debug.LogWarning("RouteInfoPanel.RefreshRouteStatusText -> routeStatusText 未绑定", this);
            return;
        }

        StringBuilder builder = new StringBuilder();
        if (RouteProgressManager.Instance == null)
        {
            builder.Append("天数：null").Append('\n');
            builder.Append("距离：null").Append('\n');
            builder.Append("地点：null");
            routeStatusText.text = builder.ToString();
            return;
        }

        int day = RouteProgressManager.Instance.GetDay();
        int distance = RouteProgressManager.Instance.GetDistance();
        MainRegionData region = RouteProgressManager.Instance.GetCurrentMainRegion();
        string name = "null";
        if (region != null)
        {
            name = string.IsNullOrWhiteSpace(region.DisplayName)
                ? region.MainRegionId.ToString()
                : region.DisplayName;
        }

        builder.Append("天数：").Append(day).Append('\n');
        builder.Append("距离：").Append(distance).Append('\n');
        builder.Append("地点：").Append(name);
        routeStatusText.text = builder.ToString();
    }

    private void OnDistanceChanged(int value)
    {
        UpdateDistance();
    }

    private void OnDayChanged(int value)
    {
        UpdateDay();
    }

    public void Show()
    {
        UpdateInfo();
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

