using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionEnemyTable", menuName = "GoHome/Region Enemy Table")]
public class RegionEnemyTable : ScriptableObject
{
    [Header("地区敌人池")]
    public List<RegionEnemyPool> regionPools = new List<RegionEnemyPool>();
}

[Serializable]
public class RegionEnemyPool
{
    [Tooltip("地区编码，格式 main_sub，例如 1_2")]
    public string regionCode;

    public List<RegionEnemyEntry> entries = new List<RegionEnemyEntry>();
}

[Serializable]
public class RegionEnemyEntry
{
    [Tooltip("敌人强引用")]
    public EnemyData enemyRef;

    [Range(0f, 100f)]
    public float weight = 1f;
}
