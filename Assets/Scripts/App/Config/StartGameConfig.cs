using System;
using UnityEngine;

[Serializable]
public class StartResourceOverrides
{
    public float health = 100;
    public float energy = 100;
    public float hunger = 80;
    public float money = 10;
}

[Serializable]
public class StartInventoryEntry
{
    public int itemId;
    public int count;
}

[CreateAssetMenu(fileName = "StartGameConfig", menuName = "GoHome/Start Game Config")]
public class StartGameConfig : ScriptableObject
{
    [Header("唯一标识（用于启动上下文传递）")]
    [SerializeField] private string presetId = "default";

    [Header("玩家模板（必填）")]
    [SerializeField] private PlayerData playerTemplate;

    [Header("开局资源覆盖（不含HP）")]
    [SerializeField] private StartResourceOverrides startResources = new StartResourceOverrides();

    [Header("开局背包")]
    [SerializeField] private StartInventoryEntry[] startInventory = Array.Empty<StartInventoryEntry>();

    [Header("玩家可见描述")]
    [TextArea]
    [SerializeField] private string description;

    public string PresetId => presetId;
    public PlayerData PlayerTemplate => playerTemplate;
    public StartResourceOverrides StartResources => startResources;
    public StartInventoryEntry[] StartInventory => startInventory ?? Array.Empty<StartInventoryEntry>();
    public string Description => description;
}

