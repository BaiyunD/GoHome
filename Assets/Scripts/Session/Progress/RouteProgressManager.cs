using System;
using System.Collections.Generic;
using UnityEngine;

public class RouteProgressManager : MonoBehaviour
{
    public static RouteProgressManager Instance { get; private set; }

    [Tooltip("Resources 下的主地区资产目录，例如：\"Regions\" 对应 Assets/Resources/Regions/")]
    [SerializeField] private string resourcesFolder = "Regions";

    private int _day = 1;
    private int _distance;
    private readonly List<MainRegionData> _mainRegions = new List<MainRegionData>();
    private MainRegionData _currentMainRegion;
    private int _currentSubRegionId;

    public event Action<int> OnDayChanged;
    public event Action<int> OnDistanceChanged;
    public event Action<MainRegionData> OnMainRegionChanged;

    public int GetDay() => _day;
    public int GetDistance() => _distance;
    public MainRegionData GetCurrentMainRegion() => _currentMainRegion;
    public int GetCurrentMainRegionId() => _currentMainRegion != null ? _currentMainRegion.MainRegionId : -1;
    public int GetCurrentSubRegionId() => _currentSubRegionId;
    public IReadOnlyList<MainRegionData> GetAllMainRegionsForValidation() => _mainRegions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("RouteProgressManager.Awake -> 检测到重复 RouteProgressManager，请确保场景中只挂载一个。");
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Initialize(int? startDistance = null, int? startDay = null)
    {
        InitializeMainRegionListFromResources();

        _distance = Mathf.Max(0, startDistance ?? 0);
        _day = Mathf.Max(1, startDay ?? 1);

        _currentMainRegion = _mainRegions.Count > 0 ? _mainRegions[0] : null;
        if (_currentMainRegion == null)
        {
            Debug.LogWarning("RouteProgressManager.Initialize -> 主地区列表为空，无法初始化当前主地区");
        }
        else
        {
            if (_currentMainRegion.StartDistance > _distance)
            {
                Debug.LogError($"初始化主地区失败：当前距离({_distance})小于主地区开始距离({_currentMainRegion.StartDistance})，地区编号：{_currentMainRegion.MainRegionId}");
            }

            IReadOnlyList<SubRegionInfo> subs = _currentMainRegion.SubRegions;
            _currentSubRegionId = subs != null && subs.Count > 0 ? subs[0].SubRegionId : 0;
        }

        OnDayChanged?.Invoke(_day);
        OnDistanceChanged?.Invoke(_distance);
        OnMainRegionChanged?.Invoke(_currentMainRegion);
    }

    public void ApplyProgressSnapshot(
        int distance,
        int day,
        int mainRegionId,
        int subRegionId
    )
    {
        Initialize(distance, day);

        MainRegionData targetMainRegion = FindMainRegionById(mainRegionId);
        if (targetMainRegion == null)
        {
            targetMainRegion = FindMainRegionByDistance(_distance);
        }

        if (targetMainRegion != null)
        {
            _currentMainRegion = targetMainRegion;
            _currentSubRegionId = ResolveSubRegionId(targetMainRegion, subRegionId);
        }

        OnDayChanged?.Invoke(_day);
        OnDistanceChanged?.Invoke(_distance);
        OnMainRegionChanged?.Invoke(_currentMainRegion);
    }

    public void Advance(int delta = 1)
    {
        ModifyDistance(delta);
    }

    public void Rest(int dayDelta = 1)
    {
        ModifyDay(dayDelta);
    }

    private void ModifyDay(int delta = 1)
    {
        if (delta == 0) return;
        int newValue = Mathf.Max(1, _day + delta);
        if (newValue == _day) return;
        _day = newValue;
        OnDayChanged?.Invoke(_day);
    }

    private void ModifyDistance(int delta = 1)
    {
        if (delta == 0) return;
        if (delta < 0 && _distance + delta < 0)
        {
            Debug.LogError($"修改距离失败：distance({_distance}) + delta({delta}) < 0");
            return;
        }

        _distance += delta;
        OnDistanceChanged?.Invoke(_distance);
        HandleDistanceChanged(_distance);
    }

    private void InitializeMainRegionListFromResources()
    {
        string folder = string.IsNullOrWhiteSpace(resourcesFolder) ? "Regions" : resourcesFolder;
        MainRegionData[] loaded = Resources.LoadAll<MainRegionData>(folder);

        _mainRegions.Clear();
        if (loaded != null && loaded.Length > 0)
        {
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] != null) _mainRegions.Add(loaded[i]);
            }
        }

        _mainRegions.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return a.MainRegionId.CompareTo(b.MainRegionId);
        });

        for (int i = 0; i < _mainRegions.Count; i++)
        {
            MainRegionData region = _mainRegions[i];
            if (region == null) continue;
            if (i != region.MainRegionId)
            {
                Debug.LogError($"地区编号错误：{region.MainRegionId}");
            }

            ValidateMainRegion(region);
        }
    }

    private void ValidateMainRegion(MainRegionData region)
    {
        if (region == null) return;

        if (region.EndDistance <= region.StartDistance || region.StartDistance < 0)
        {
            Debug.LogError($"主地区距离段错误：id={region.MainRegionId}, start={region.StartDistance}, end={region.EndDistance}");
        }

        IReadOnlyList<SubRegionInfo> subs = region.SubRegions;
        if (subs == null || subs.Count == 0)
        {
            Debug.LogError($"主地区分地区列表为空：id={region.MainRegionId}, start={region.StartDistance}, end={region.EndDistance}");
        }
    }

    private void HandleDistanceChanged(int newDistance)
    {
        if (_currentMainRegion == null)
        {
            return;
        }

        int currentIndex = _currentMainRegion.MainRegionId;
        int start = _currentMainRegion.StartDistance;
        int end = _currentMainRegion.EndDistance;

        if (newDistance == end + 1)
        {
            int nextIndex = currentIndex + 1;
            if (nextIndex >= 0 && nextIndex < _mainRegions.Count)
            {
                SetCurrentMainRegion(_mainRegions[nextIndex]);
            }
            else
            {
                Debug.Log($"RouteProgressManager -> 已到最后一个主地区，distance={newDistance}");
            }

            return;
        }

        if (newDistance >= start && newDistance <= end)
        {
            return;
        }

        int step = newDistance < start ? -1 : 1;
        int seek = currentIndex;
        while (true)
        {
            seek += step;
            if (seek < 0 || seek >= _mainRegions.Count)
            {
                Debug.Log($"RouteProgressManager -> 未找到匹配主地区，distance={newDistance}");
                return;
            }

            MainRegionData region = _mainRegions[seek];
            if (region == null) continue;
            if (newDistance >= region.StartDistance && newDistance <= region.EndDistance)
            {
                SetCurrentMainRegion(region);
                return;
            }
        }
    }

    private void SetCurrentMainRegion(MainRegionData region)
    {
        if (region == null || region == _currentMainRegion)
        {
            return;
        }

        _currentMainRegion = region;
        IReadOnlyList<SubRegionInfo> subs = _currentMainRegion.SubRegions;
        _currentSubRegionId = subs != null && subs.Count > 0 ? subs[0].SubRegionId : 0;
        OnMainRegionChanged?.Invoke(_currentMainRegion);
    }

    private MainRegionData FindMainRegionById(int mainRegionId)
    {
        if (mainRegionId < 0)
        {
            return null;
        }

        for (int i = 0; i < _mainRegions.Count; i++)
        {
            MainRegionData region = _mainRegions[i];
            if (region != null && region.MainRegionId == mainRegionId)
            {
                return region;
            }
        }

        return null;
    }

    private MainRegionData FindMainRegionByDistance(int distance)
    {
        for (int i = 0; i < _mainRegions.Count; i++)
        {
            MainRegionData region = _mainRegions[i];
            if (region == null)
            {
                continue;
            }

            if (distance >= region.StartDistance && distance <= region.EndDistance)
            {
                return region;
            }
        }

        return _mainRegions.Count > 0 ? _mainRegions[0] : null;
    }

    private int ResolveSubRegionId(MainRegionData region, int expectedSubRegionId)
    {
        if (region == null || region.SubRegions == null || region.SubRegions.Count == 0)
        {
            return 0;
        }

        for (int i = 0; i < region.SubRegions.Count; i++)
        {
            SubRegionInfo subRegion = region.SubRegions[i];
            if (subRegion != null && subRegion.SubRegionId == expectedSubRegionId)
            {
                return expectedSubRegionId;
            }
        }

        return region.SubRegions[0].SubRegionId;
    }

    public string GetCurrentSubRegionName()
    {
        if (_currentMainRegion == null) return string.Empty;
        IReadOnlyList<SubRegionInfo> subs = _currentMainRegion.SubRegions;
        if (subs == null || subs.Count == 0) return string.Empty;

        for (int i = 0; i < subs.Count; i++)
        {
            SubRegionInfo info = subs[i];
            if (info != null && info.SubRegionId == _currentSubRegionId)
            {
                return info.SubRegionName;
            }
        }

        return string.Empty;
    }
}
