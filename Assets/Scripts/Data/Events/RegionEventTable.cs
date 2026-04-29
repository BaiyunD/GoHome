using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionEventTable", menuName = "GoHome/Region Event Table")]
public class RegionEventTable : ScriptableObject
{
    [Header("地区事件池")]
    public List<RegionEventPool> regionPools = new List<RegionEventPool>();
}

[Serializable]
public class RegionEventPool
{
    [Tooltip("地区编码，格式 main_sub，例如 1_2")]
    public string regionCode;

    public List<RegionEventEntry> entries = new List<RegionEventEntry>();
}

[Serializable]
public class RegionEventEntry
{
    [Tooltip("事件强引用")]
    public GameEvent eventRef;

    [Range(0f, 100f)]
    public float weight = 1f;
}
