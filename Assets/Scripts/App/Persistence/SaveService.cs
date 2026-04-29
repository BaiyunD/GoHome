using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int Version = 2;
    public SavePlayerData Player = new SavePlayerData();
    public SaveRouteData Route = new SaveRouteData();
    public SaveInventoryData Inventory = new SaveInventoryData();
    public SaveTraitData Traits = new SaveTraitData();
    public SaveItemData Items = new SaveItemData();
    public SaveShopData Shop = new SaveShopData();
}

[Serializable]
public class SavePlayerData
{
    public float HPCurrent;
    public float HPMax;
    public float HungerCurrent;
    public float HungerMax;
    public float EnergyCurrent;
    public float EnergyMax;
    public float HealthCurrent;
    public float HealthMax;
    public float Money;
    public float Attack;
    public float Defense;
    public float CriticalRate;
    public float CriticalDamage;
    public float BlockRate;
    public float DodgeRate;
}

[Serializable]
public class SaveRouteData
{
    public int Day = 1;
    public int Distance;
    public int MainRegionId = -1;
    public int SubRegionId = -1;
}

[Serializable]
public class SaveInventoryEntryData
{
    public int ItemId;
    public int Count;
}

[Serializable]
public class SaveInventoryData
{
    public SaveInventoryEntryData[] Entries = Array.Empty<SaveInventoryEntryData>();
}

[Serializable]
public class SaveTraitData
{
    public string[] PlayerTraitIds = Array.Empty<string>();
}

[Serializable]
public class SaveItemData
{
    public SaveConsumableData Consumables = new SaveConsumableData();
}

[Serializable]
public class SaveConsumableData
{
    public int MedkitHealBonus;
}

[Serializable]
public class SaveShopData
{
    public int Points;
    public SaveShopBuyPriceData[] BuyPriceSnapshots = Array.Empty<SaveShopBuyPriceData>();
}

[Serializable]
public class SaveShopBuyPriceData
{
    public int CommodityId;
    public float CurrentBuyPrice;
}

public interface ISaveService
{
    void Save(SaveData saveData);
    SaveData Load();
    bool HasSave();
    void Delete();
    bool IsCorruptedHandling();
}

public class SaveService : ISaveService
{
    private const string SAVE_FILE_NAME = "save_slot_1.json";
    private const string CORRUPT_SUFFIX = ".corrupted";
    private readonly string _savePath;

    public SaveService()
    {
        _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    public void Save(SaveData saveData)
    {
        if (saveData == null)
        {
            saveData = new SaveData();
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(_savePath, json);
    }

    public SaveData Load()
    {
        if (!File.Exists(_savePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(_savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                IsCorruptedHandling();
                return null;
            }

            return saveData;
        }
        catch (Exception)
        {
            IsCorruptedHandling();
            return null;
        }
    }

    public bool HasSave()
    {
        return File.Exists(_savePath);
    }

    public void Delete()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
        }
    }

    public bool IsCorruptedHandling()
    {
        if (!File.Exists(_savePath))
        {
            return false;
        }

        try
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string corruptPath = _savePath + "." + timestamp + CORRUPT_SUFFIX;
            File.Move(_savePath, corruptPath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
