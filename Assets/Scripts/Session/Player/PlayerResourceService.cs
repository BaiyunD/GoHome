using System;
using UnityEngine;

public enum PlayerResourceType
{
    HP = 0,
    Health = 1,
    Hunger = 2,
    Energy = 3,
    Money = 4
}

public class PlayerResourceService : MonoBehaviour
{
    public static PlayerResourceService Instance
    {
        get; private set;
    }

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

    public bool TryGetValue(PlayerResourceType type, out float value)
    {
        value = 0f;
        if (type == PlayerResourceType.HP)
        {
            if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
            {
                Debug.LogWarning("PlayerResourceService.TryGetValue -> 玩家运行态未就绪。");
                return false;
            }

            value = PlayerStateManager.Instance.CurrentHp;
            return true;
        }
        if (type == PlayerResourceType.Money)
        {
            if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
            {
                Debug.LogWarning("PlayerResourceService.TryGetValue -> 玩家运行态未就绪。");
                return false;
            }

            value = PlayerStateManager.Instance.Current.Money;
            return true;
        }

        if (type == PlayerResourceType.Health || type == PlayerResourceType.Energy || type == PlayerResourceType.Hunger)
        {
            if (SurvivalResourceManager.Instance == null)
            {
                Debug.LogWarning("PlayerResourceService.TryGetValue -> SurvivalResourceManager 未就绪。");
                return false;
            }

            SurvivalResourceType mapped = MapSurvivalType(type);
            return SurvivalResourceManager.Instance.TryGetValue(mapped, out value);
        }

        DebugLogUnsupportedType("TryGetValue", type, string.Empty);
        return false;
    }

    public bool TryGetMaxValue(PlayerResourceType type, out float value)
    {
        value = 0f;
        if (type == PlayerResourceType.HP)
        {
            if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
            {
                Debug.LogWarning("PlayerResourceService.TryGetMaxValue -> 玩家运行态未就绪。");
                return false;
            }

            value = PlayerStateManager.Instance.Current.MaxHp;
            return true;
        }

        if (type == PlayerResourceType.Health || type == PlayerResourceType.Energy || type == PlayerResourceType.Hunger)
        {
            if (SurvivalResourceManager.Instance == null)
            {
                Debug.LogWarning("PlayerResourceService.TryGetMaxValue -> SurvivalResourceManager 未就绪。");
                return false;
            }

            SurvivalResourceType mapped = MapSurvivalType(type);
            return SurvivalResourceManager.Instance.TryGetMaxValue(mapped, out value);
        }

        DebugLogUnsupportedType("TryGetMaxValue", type, string.Empty);
        return false;
    }

    public bool TrySetValue(PlayerResourceType type, float value, string reason)
    {
        if (type == PlayerResourceType.HP)
        {
            if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
            {
                Debug.LogWarning($"PlayerResourceService.TrySetValue -> 玩家运行态未就绪，reason={reason}");
                return false;
            }

            PlayerStateManager.Instance.CurrentHp = value;
            return true;
        }
        if (type == PlayerResourceType.Money)
        {
            if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
            {
                Debug.LogWarning($"PlayerResourceService.TrySetValue -> 玩家运行态未就绪，reason={reason}");
                return false;
            }

            PlayerStateManager.Instance.Current.Money = value;
            return true;
        }

        if (type == PlayerResourceType.Health || type == PlayerResourceType.Energy || type == PlayerResourceType.Hunger)
        {
            if (SurvivalResourceManager.Instance == null)
            {
                Debug.LogWarning($"PlayerResourceService.TrySetValue -> SurvivalResourceManager 未就绪，reason={reason}");
                return false;
            }

            SurvivalResourceType mapped = MapSurvivalType(type);
            return SurvivalResourceManager.Instance.TrySetValue(mapped, value, reason);
        }

        DebugLogUnsupportedType("TrySetValue", type, reason);
        return false;
    }

    public bool ApplyDelta(PlayerResourceType type, float delta, string reason)
    {
        if (!TryGetValue(type, out float currentValue))
        {
            return false;
        }

        return TrySetValue(type, currentValue + delta, reason);
    }

    public bool TryConsume(PlayerResourceType type, float amount, string reason)
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

    public bool TrySpendMoney(float amount, string reason)
    {
        if (amount < 0f)
        {
            return ApplyDelta(PlayerResourceType.Money, -amount, reason);
        }

        return TryConsume(PlayerResourceType.Money, amount, reason);
    }

    private static void DebugLogUnsupportedType(string method, PlayerResourceType type, string reason)
    {
        Debug.LogWarning(
            $"PlayerResourceService.{method} -> 资源类型暂未接入：type={type} reason={reason}"
        );
    }

    private static SurvivalResourceType MapSurvivalType(PlayerResourceType type)
    {
        switch (type)
        {
            case PlayerResourceType.Health:
                return SurvivalResourceType.Health;
            case PlayerResourceType.Energy:
                return SurvivalResourceType.Energy;
            case PlayerResourceType.Hunger:
                return SurvivalResourceType.Hunger;
            default:
                return SurvivalResourceType.Health;
        }
    }
}

