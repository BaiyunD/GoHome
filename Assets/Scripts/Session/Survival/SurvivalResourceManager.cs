using System;
using UnityEngine;

public enum SurvivalResourceType
{
    Health = 0,
    Energy = 1,
    Hunger = 2
}

public sealed class SurvivalResourceManager : MonoBehaviour
{
    public static SurvivalResourceManager Instance
    {
        get; private set;
    }

    public float HealthCurrent { get; private set; }
    public float EnergyCurrent { get; private set; }
    public float EnergyMax { get; private set; }
    public float HungerCurrent { get; private set; }
    public float HungerMax { get; private set; }

    public event Action<SurvivalResourceType> ResourceChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void InitializeFromStartConfig(StartGameConfig startGameConfig)
    {
        StartResourceOverrides startResources = startGameConfig != null ? startGameConfig.StartResources : null;

        float health = startResources != null ? startResources.health : 100f;
        float energy = startResources != null ? startResources.energy : 100f;
        float hunger = startResources != null ? startResources.hunger : 80f;

        SetHealthCurrent(health);
        SetMaxAndCurrent(SurvivalResourceType.Energy, energy);
        SetMaxAndCurrent(SurvivalResourceType.Hunger, hunger);
    }

    public void ApplySnapshot(SavePlayerData snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        SetHealthCurrent(snapshot.HealthCurrent);
        SetMaxAndCurrent(SurvivalResourceType.Energy, snapshot.EnergyMax, snapshot.EnergyCurrent);
        SetMaxAndCurrent(SurvivalResourceType.Hunger, snapshot.HungerMax, snapshot.HungerCurrent);
    }

    public void FillSnapshot(SavePlayerData snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        snapshot.HealthCurrent = HealthCurrent;
        snapshot.HealthMax = 0f;
        snapshot.EnergyCurrent = EnergyCurrent;
        snapshot.EnergyMax = EnergyMax;
        snapshot.HungerCurrent = HungerCurrent;
        snapshot.HungerMax = HungerMax;
    }

    public bool TryGetValue(SurvivalResourceType type, out float value)
    {
        switch (type)
        {
            case SurvivalResourceType.Health:
                value = HealthCurrent;
                return true;
            case SurvivalResourceType.Energy:
                value = EnergyCurrent;
                return true;
            case SurvivalResourceType.Hunger:
                value = HungerCurrent;
                return true;
            default:
                value = 0f;
                return false;
        }
    }

    public bool TryGetMaxValue(SurvivalResourceType type, out float value)
    {
        switch (type)
        {
            case SurvivalResourceType.Health:
                value = 0f;
                return false;
            case SurvivalResourceType.Energy:
                value = EnergyMax;
                return true;
            case SurvivalResourceType.Hunger:
                value = HungerMax;
                return true;
            default:
                value = 0f;
                return false;
        }
    }

    public bool TrySetMaxValue(SurvivalResourceType type, float maxValue, string reason)
    {
        float safeMax = Mathf.Max(0f, maxValue);
        switch (type)
        {
            case SurvivalResourceType.Energy:
                EnergyMax = safeMax;
                EnergyCurrent = ClampCurrent(EnergyCurrent, EnergyMax);
                ResourceChanged?.Invoke(type);
                return true;
            case SurvivalResourceType.Hunger:
                HungerMax = safeMax;
                HungerCurrent = ClampCurrent(HungerCurrent, HungerMax);
                ResourceChanged?.Invoke(type);
                return true;
            default:
                Debug.LogWarning($"SurvivalResourceManager.TrySetMaxValue -> 未处理 type={type} reason={reason}");
                return false;
        }
    }

    public bool TrySetValue(SurvivalResourceType type, float value, string reason)
    {
        switch (type)
        {
            case SurvivalResourceType.Health:
                HealthCurrent = Mathf.Max(0f, value);
                ResourceChanged?.Invoke(type);
                return true;
            case SurvivalResourceType.Energy:
                EnergyCurrent = ClampCurrent(value, EnergyMax);
                ResourceChanged?.Invoke(type);
                return true;
            case SurvivalResourceType.Hunger:
                HungerCurrent = ClampCurrent(value, HungerMax);
                ResourceChanged?.Invoke(type);
                return true;
            default:
                Debug.LogWarning($"SurvivalResourceManager.TrySetValue -> 未处理 type={type} reason={reason}");
                return false;
        }
    }

    public bool ApplyDelta(SurvivalResourceType type, float delta, string reason)
    {
        if (!TryGetValue(type, out float currentValue))
        {
            return false;
        }

        return TrySetValue(type, currentValue + delta, reason);
    }

    public bool TryConsume(SurvivalResourceType type, float amount, string reason)
    {
        if (amount < 0f)
        {
            return ApplyDelta(type, -amount, reason);
        }

        if (!TryGetValue(type, out float currentValue))
        {
            return false;
        }

        if (currentValue < amount)
        {
            return false;
        }

        return TrySetValue(type, currentValue - amount, reason);
    }

    private void SetMaxAndCurrent(SurvivalResourceType type, float maxAndCurrent)
    {
        SetMaxAndCurrent(type, maxAndCurrent, maxAndCurrent);
    }

    private void SetMaxAndCurrent(SurvivalResourceType type, float max, float current)
    {
        float safeMax = Mathf.Max(0f, max);
        float safeCurrent = ClampCurrent(current, safeMax);

        switch (type)
        {
            case SurvivalResourceType.Health:
                SetHealthCurrent(safeCurrent);
                break;
            case SurvivalResourceType.Energy:
                EnergyMax = safeMax;
                EnergyCurrent = safeCurrent;
                ResourceChanged?.Invoke(type);
                break;
            case SurvivalResourceType.Hunger:
                HungerMax = safeMax;
                HungerCurrent = safeCurrent;
                ResourceChanged?.Invoke(type);
                break;
        }
    }

    private static float ClampCurrent(float current, float max)
    {
        float safeMax = Mathf.Max(0f, max);
        return Mathf.Clamp(current, 0f, safeMax);
    }

    private void SetHealthCurrent(float value)
    {
        HealthCurrent = Mathf.Max(0f, value);
        ResourceChanged?.Invoke(SurvivalResourceType.Health);
    }
}

