using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMainRegion", menuName = "GoHome/Regions/MainRegion")]
public class MainRegionData : ScriptableObject
{
    [Header("主地区")]
    [SerializeField] private string mainRegionKey;
    [SerializeField] private int mainRegionId;
    [SerializeField] private string displayName;

    [Header("距离段（命中规则：start<=distance<=end）")]
    [SerializeField] private int startDistance;
    [SerializeField] private int endDistance;

    [Header("分地区列表")]
    [SerializeField] private List<SubRegionInfo> subRegions = new List<SubRegionInfo>();

    public string MainRegionKey => string.IsNullOrWhiteSpace(mainRegionKey)
        ? $"main_{mainRegionId}"
        : mainRegionKey.Trim();
    public int MainRegionId => mainRegionId;
    public string DisplayName => displayName;
    public int StartDistance => startDistance;
    public int EndDistance => endDistance;
    public IReadOnlyList<SubRegionInfo> SubRegions => subRegions;
}

[System.Serializable]
public class SubRegionInfo
{
    [SerializeField] private string subRegionName;
    [SerializeField] private string subRegionKey;
    [SerializeField] private int subRegionId;

    public string SubRegionName => subRegionName;
    public string SubRegionKey => string.IsNullOrWhiteSpace(subRegionKey)
        ? $"sub_{subRegionId}"
        : subRegionKey.Trim();
    public int SubRegionId => subRegionId;
}
