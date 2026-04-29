using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RegionLootTable", menuName = "GoHome/Region Loot Table")]
public class RegionLootTable : ScriptableObject
{
    [Header("掉落种类数概率（总和建议100）")]
    [Range(0f, 100f)] public float noLootProbability = 20f;
    [Range(0f, 100f)] public float oneLootProbability = 50f;
    [Range(0f, 100f)] public float twoLootProbability = 20f;
    [Range(0f, 100f)] public float threeLootProbability = 10f;

    [Header("地区池")]
    public List<RegionLootPool> regionPools = new List<RegionLootPool>();
}

[Serializable]
public class RegionLootPool
{
    [Tooltip("地区编码，格式 main_sub，例如 1_2")]
    public string regionCode;

    public List<LootEntry> entries = new List<LootEntry>();
}

[Serializable]
public class LootEntry
{
    public LootRewardType rewardType = LootRewardType.Item;

    [Tooltip("rewardType=Item 时填写物品ID")]
    public int itemId;

    [Tooltip("rewardType=Money 时填写发放金额")]
    public float moneyAmount = 1f;

    [Range(0f, 100f)] public float weight = 1f;
    public LootCountOption countOption = LootCountOption.One;
}

public enum LootRewardType
{
    Item = 0,
    Money = 1
}

public enum LootCountOption
{
    One = 1,
    Two = 2
}
