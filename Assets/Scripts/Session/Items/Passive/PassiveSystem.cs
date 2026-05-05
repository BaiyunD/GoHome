using UnityEngine;

public class PassiveSystem : MonoBehaviour
{
    private float _lastAppliedEnergyMaxBonus;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged += Rebuild;
        }

        if (CraftManager.Instance != null)
        {
            CraftManager.Instance.OnCrafted += Rebuild;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= Rebuild;
        }

        if (CraftManager.Instance != null)
        {
            CraftManager.Instance.OnCrafted -= Rebuild;
        }
    }

    /// <summary>
    /// 根据当前背包聚合道具被动（不写入 PlayerRuntime）。
    /// </summary>
    public static PassiveAccumulator ComputePassiveAccumulatorFromInventory()
    {
        PassiveAccumulator acc = new PassiveAccumulator();
        if (InventoryManager.Instance == null || ItemRegistry.Instance == null)
        {
            return acc;
        }

        foreach (System.Collections.Generic.KeyValuePair<int, int> pair in InventoryManager.inventoryDict)
        {
            if (pair.Value <= 0)
            {
                continue;
            }

            if (!ItemRegistry.Instance.TryGet(pair.Key, out ItemBase item) || item == null)
            {
                continue;
            }

            if (item.Kind != ItemKind.Tool)
            {
                continue;
            }

            ItemEffectDispatcher.OnPassiveItemEffect(pair.Key, pair.Value, acc, out _);
        }

        return acc;
    }

    public void Rebuild()
    {
        if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
        {
            return;
        }

        PassiveAccumulator acc = new PassiveAccumulator();
        if (InventoryManager.Instance != null && ItemRegistry.Instance != null)
        {
            foreach (System.Collections.Generic.KeyValuePair<int, int> pair in InventoryManager.inventoryDict)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                if (!ItemRegistry.Instance.TryGet(pair.Key, out ItemBase item) || item == null)
                {
                    continue;
                }

                if (item.Kind != ItemKind.Tool)
                {
                    continue;
                }

                bool applied = ItemEffectDispatcher.OnPassiveItemEffect(pair.Key, pair.Value, acc, out ItemEffectSource source);
                if (!applied)
                {
                    continue;
                }

                Debug.Log($"PassiveSystem.Rebuild -> item={pair.Key}, source={source}, level={pair.Value}");
            }
        }

        ApplyPassiveLayerOnly(PlayerStateManager.Instance.Current, acc);
    }

    /// <summary>只更新道具被动层并重算最终扁平字段；不改 CombatBase。</summary>
    private void ApplyPassiveLayerOnly(PlayerRuntime runtime, PassiveAccumulator acc)
    {
        if (runtime == null || acc == null)
        {
            return;
        }

        runtime.CombatItemPassive = PlayerCombatStatItemPassive.FromAccumulator(acc);
        runtime.RefreshFlattenedCombatFromLayers();

        ApplyEnergyMaxBonus(acc.EnergyMaxBonus);

        if (acc.HungerMaxBonus != 0f)
        {
            Debug.LogWarning(
                "PassiveSystem.ApplyPassiveLayerOnly -> Hunger 被动加成暂未接入资源系统，已跳过。"
            );
        }
    }

    private void ApplyEnergyMaxBonus(float energyMaxBonus)
    {
        if (SurvivalResourceManager.Instance == null)
        {
            Debug.LogWarning("PassiveSystem.ApplyEnergyMaxBonus -> SurvivalResourceManager 未就绪。");
            return;
        }

        if (!SurvivalResourceManager.Instance.TryGetMaxValue(SurvivalResourceType.Energy, out float baseMax))
        {
            Debug.LogWarning("PassiveSystem.ApplyEnergyMaxBonus -> 读取 EnergyMax 失败。");
            return;
        }

        float baseWithoutBonus = Mathf.Max(0f, baseMax - _lastAppliedEnergyMaxBonus);
        float finalMax = Mathf.Max(0f, baseWithoutBonus + energyMaxBonus);
        SurvivalResourceManager.Instance.TrySetMaxValue(
            SurvivalResourceType.Energy,
            finalMax,
            "PassiveSystem.ApplyEnergyMaxBonus"
        );
        _lastAppliedEnergyMaxBonus = energyMaxBonus;
        Debug.Log(
            $"PassiveSystem.ApplyEnergyMaxBonus -> baseMax={baseWithoutBonus}, bonus={energyMaxBonus}, finalMax={finalMax}"
        );
    }
}
