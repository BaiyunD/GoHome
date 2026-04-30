using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveSnapshotAssembler
{
    public SaveData Assemble()
    {
        SaveData saveData = new SaveData();
        FillPlayer(saveData.Player);
        FillRoute(saveData.Route);
        FillInventory(saveData.Inventory);
        FillTraits(saveData.Traits);
        FillItems(saveData.Items);
        FillShop(saveData.Shop);
        return saveData;
    }

    public bool Apply(SaveData saveData, out string errorMessage)
    {
        if (saveData == null)
        {
            errorMessage = "存档为空。";
            return false;
        }

        if (GameManager.Instance == null)
        {
            errorMessage = "GameManager 未就绪。";
            return false;
        }

        if (saveData.Player == null || saveData.Route == null)
        {
            errorMessage = "存档缺少关键字段（Player/Route）。";
            return false;
        }

        EnsurePlayerInitialized();
        ApplyPlayer(saveData.Player);
        ApplyRoute(saveData.Route);
        ApplyInventory(saveData.Inventory);
        ApplyTraits(saveData.Traits);
        ApplyItems(saveData.Items);
        ApplyShop(saveData.Shop);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateInfo();
        }

        errorMessage = string.Empty;
        return true;
    }

    private static void FillPlayer(SavePlayerData playerData)
    {
        if (playerData == null || PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
        {
            return;
        }

        PlayerRuntime runtime = PlayerStateManager.Instance.Current;
        playerData.HPCurrent = runtime.CurrentHp;
        playerData.HPMax = runtime.MaxHp;
        if (SurvivalResourceManager.Instance != null)
        {
            SurvivalResourceManager.Instance.FillSnapshot(playerData);
        }
        else
        {
            playerData.HungerCurrent = 0f;
            playerData.HungerMax = 0f;
            playerData.EnergyCurrent = 0f;
            playerData.EnergyMax = 0f;
            playerData.HealthCurrent = 0f;
            playerData.HealthMax = 0f;
        }
        playerData.MoneyCents = runtime.MoneyCents;
        playerData.Attack = runtime.Attack;
        playerData.Defense = runtime.Defense;
        playerData.CriticalRate = runtime.CriticalRate;
        playerData.CriticalDamage = runtime.CriticalDamage;
        playerData.BlockRate = runtime.BlockRate;
        playerData.DodgeRate = runtime.DodgeRate;
    }

    private static void FillRoute(SaveRouteData routeData)
    {
        if (routeData == null || RouteProgressManager.Instance == null)
        {
            return;
        }

        routeData.Day = RouteProgressManager.Instance.GetDay();
        routeData.Distance = RouteProgressManager.Instance.GetDistance();
        routeData.MainRegionId = RouteProgressManager.Instance.GetCurrentMainRegionId();
        routeData.SubRegionId = RouteProgressManager.Instance.GetCurrentSubRegionId();
    }

    private static void FillInventory(SaveInventoryData inventoryData)
    {
        if (inventoryData == null || InventoryManager.Instance == null)
        {
            return;
        }

        List<SaveInventoryEntryData> entries = InventoryManager.Instance.ExportEntries();
        inventoryData.Entries = entries.ToArray();
    }

    private static void FillTraits(SaveTraitData traitData)
    {
        if (traitData == null || TraitManager.Instance == null)
        {
            return;
        }

        traitData.PlayerTraitIds = TraitManager.Instance.ExportPlayerTraitIds().ToArray();
    }

    private static void FillItems(SaveItemData itemData)
    {
        if (itemData == null)
        {
            return;
        }

        if (itemData.Consumables == null)
        {
            itemData.Consumables = new SaveConsumableData();
        }

        ConsumableRuntimeState.FillSnapshot(itemData.Consumables);
    }

    private static void FillShop(SaveShopData shopData)
    {
        if (shopData == null || ShopService.Instance == null)
        {
            return;
        }

        ShopWalletSnapshot snapshot = ShopService.Instance.ExportSnapshot();
        shopData.Points = snapshot.Points;
        ShopBuyPriceSnapshot[] buyPriceSnapshots = snapshot.BuyPriceSnapshots ?? Array.Empty<ShopBuyPriceSnapshot>();
        SaveShopBuyPriceData[] mappedSnapshots = new SaveShopBuyPriceData[buyPriceSnapshots.Length];
        for (int i = 0; i < buyPriceSnapshots.Length; i++)
        {
            mappedSnapshots[i] = new SaveShopBuyPriceData
            {
                CommodityId = buyPriceSnapshots[i].CommodityId,
                CurrentBuyPrice = buyPriceSnapshots[i].CurrentBuyPrice
            };
        }

        shopData.BuyPriceSnapshots = mappedSnapshots;
    }

    private static void EnsurePlayerInitialized()
    {
        if (PlayerStateManager.Instance == null)
        {
            return;
        }
        if (PlayerStateManager.Instance.Current == null)
        {
            StartGameConfig fallbackStartGameConfig = ResolveFallbackStartGameConfig();
            if (fallbackStartGameConfig != null)
            {
                PlayerStateManager.Instance.NewGame(fallbackStartGameConfig, out _);
            }
        }
    }

    private static void ApplyPlayer(SavePlayerData playerData)
    {
        if (playerData == null || PlayerStateManager.Instance == null)
        {
            return;
        }

        PlayerStateManager.Instance.ApplySaveSnapshot(playerData, out _);
    }

    private static void ApplyRoute(SaveRouteData routeData)
    {
        if (routeData == null || RouteProgressManager.Instance == null)
        {
            return;
        }

        RouteProgressManager.Instance.ApplyProgressSnapshot(
            routeData.Distance,
            routeData.Day,
            routeData.MainRegionId,
            routeData.SubRegionId
        );
    }

    private static void ApplyInventory(SaveInventoryData inventoryData)
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        SaveInventoryEntryData[] entries = inventoryData != null
            ? inventoryData.Entries
            : Array.Empty<SaveInventoryEntryData>();
        InventoryManager.Instance.ReplaceAllItems(entries);
    }

    private static void ApplyTraits(SaveTraitData traitData)
    {
        if (TraitManager.Instance == null)
        {
            return;
        }

        string[] traitIds = traitData != null
            ? traitData.PlayerTraitIds
            : Array.Empty<string>();
        TraitManager.Instance.ReplacePlayerTraits(traitIds);
    }

    private static void ApplyItems(SaveItemData itemData)
    {
        SaveConsumableData consumableData = itemData != null
            ? itemData.Consumables
            : null;
        ConsumableRuntimeState.ApplySnapshot(consumableData);
    }

    private static void ApplyShop(SaveShopData shopData)
    {
        if (ShopService.Instance == null)
        {
            return;
        }

        ShopWalletSnapshot snapshot = new ShopWalletSnapshot
        {
            Points = shopData != null ? shopData.Points : 0,
            BuyPriceSnapshots = MapBuyPriceSnapshots(shopData)
        };
        ShopService.Instance.ApplySnapshot(snapshot);
    }

    public static ShopBuyPriceSnapshot[] MapBuyPriceSnapshots(SaveShopData shopData)
    {
        SaveShopBuyPriceData[] saveSnapshots = shopData != null
            ? shopData.BuyPriceSnapshots
            : null;
        if (saveSnapshots == null || saveSnapshots.Length == 0)
        {
            return Array.Empty<ShopBuyPriceSnapshot>();
        }

        ShopBuyPriceSnapshot[] mapped = new ShopBuyPriceSnapshot[saveSnapshots.Length];
        for (int i = 0; i < saveSnapshots.Length; i++)
        {
            SaveShopBuyPriceData saveSnapshot = saveSnapshots[i];
            if (saveSnapshot == null)
            {
                continue;
            }

            mapped[i] = new ShopBuyPriceSnapshot
            {
                CommodityId = saveSnapshot.CommodityId,
                CurrentBuyPrice = saveSnapshot.CurrentBuyPrice
            };
        }

        return mapped;
    }

    private static StartGameConfig ResolveFallbackStartGameConfig()
    {
        if (AppRoot.Instance == null || AppRoot.Instance.Configs == null)
        {
            return null;
        }

        IReadOnlyList<StartGameConfig> presets = AppRoot.Instance.Configs.GetStartGameConfigs();
        for (int i = 0; i < presets.Count; i++)
        {
            StartGameConfig preset = presets[i];
            if (preset != null && preset.PlayerTemplate != null)
            {
                return preset;
            }
        }

        return null;
    }

}

public static class ConsumableRuntimeState
{
    public static int MedkitHealBonus { get; private set; }

    public static void ResetForNewGame()
    {
        MedkitHealBonus = 0;
    }

    public static void ApplySnapshot(SaveConsumableData snapshot)
    {
        MedkitHealBonus = snapshot != null ? snapshot.MedkitHealBonus : 0;
        if (MedkitHealBonus < 0)
        {
            MedkitHealBonus = 0;
        }
    }

    public static void FillSnapshot(SaveConsumableData snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        snapshot.MedkitHealBonus = MedkitHealBonus;
    }

    public static void IncreaseMedkitHealBonus(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        MedkitHealBonus += amount;
    }
}
