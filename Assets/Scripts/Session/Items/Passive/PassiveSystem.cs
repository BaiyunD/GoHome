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

    public void Rebuild()
    {
        if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
        {
            return;
        }

        if (ItemRegistry.Instance == null)
        {
            return;
        }

        PassiveAccumulator acc = new PassiveAccumulator();

        foreach (var pair in InventoryManager.inventoryDict)
        {
            if (pair.Value <= 0) continue;
            if (!ItemRegistry.Instance.TryGet(pair.Key, out ItemBase item) || item == null) continue;
            if (item.Kind != ItemKind.Tool) continue;

            bool applied = ItemEffectDispatcher.OnPassiveItemEffect(pair.Key, pair.Value, acc, out ItemEffectSource source);
            if (!applied)
            {
                continue;
            }

            Debug.Log($"PassiveSystem.Rebuild -> item={pair.Key}, source={source}, level={pair.Value}");
        }

        ApplyToPlayer(
            PlayerStateManager.Instance.Current,
            PlayerStateManager.Instance.CurrentStartGameConfig,
            acc
        );
    }

    private void ApplyToPlayer(PlayerRuntime runtime, StartGameConfig startGameConfig, PassiveAccumulator acc)
    {
        if (runtime == null || acc == null)
        {
            return;
        }
        PlayerData template = runtime.RuntimeData;

        float baseHpMax = template != null ? template.HP : 100f;
        runtime.MaxHp = baseHpMax + acc.HpMaxBonus;
        runtime.CurrentHp = Mathf.Clamp(runtime.CurrentHp, 0f, Mathf.Max(0f, runtime.MaxHp));

        float baseAttack = template != null ? template.Attack : 0f;
        float baseDefense = template != null ? template.Defense : 0f;
        float baseCriticalRate = template != null ? template.CriticalRate : 0f;
        float baseCriticalDamage = template != null ? template.CriticalDamage : 150f;
        float baseBlockRate = template != null ? template.BlockRate : 0f;
        float baseDodgeRate = template != null ? template.DodgeRate : 0f;

        runtime.Attack = baseAttack + acc.AttackBonus;
        runtime.Defense = baseDefense + acc.DefenseBonus;
        runtime.CriticalRate = baseCriticalRate + acc.CriticalRateBonus;
        runtime.CriticalDamage = baseCriticalDamage;
        runtime.BlockRate = baseBlockRate + acc.BlockRateBonus;
        runtime.DodgeRate = baseDodgeRate + acc.DodgeRateBonus;

        ApplyEnergyMaxBonus(acc.EnergyMaxBonus);

        if (acc.HungerMaxBonus != 0f)
        {
            Debug.LogWarning(
                "PassiveSystem.ApplyToPlayer -> Hunger 被动加成暂未接入资源系统，已跳过。"
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

