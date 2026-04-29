using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatResultHelper
{
    /// <summary>
    /// 应用所有结果（同步）
    /// </summary>
    public static void ApplyResult(StatResult result)
    {
        var playerRuntime = PlayerStateManager.Instance != null && PlayerStateManager.Instance.Current != null
            ? PlayerStateManager.Instance.Current
            : null;
        if (playerRuntime == null)
        {
            return;
        }

        switch (result.type)
        {
            case ResultType.HP:
                ApplyToPlayerValue(
                    () => playerRuntime.CurrentHp,
                    value => playerRuntime.CurrentHp = value,
                    playerRuntime.MaxHp,
                    result
                );
                break;
            case ResultType.Health:
                Debug.LogWarning("StatResultHelper -> Health 当前未接入运行态，已跳过。");
                break;
            case ResultType.Hunger:
                Debug.LogWarning("StatResultHelper -> Hunger 当前未接入运行态，已跳过。");
                break;
            case ResultType.Energy:
                Debug.LogWarning("StatResultHelper -> Energy 当前未接入运行态，已跳过。");
                break;
            case ResultType.Money:
                Debug.LogWarning("StatResultHelper -> Money 当前未接入运行态，已跳过。");
                break;
            case ResultType.Attack:
                ApplyToPlayerValue(
                    () => playerRuntime.Attack,
                    value => playerRuntime.Attack = value,
                    null,
                    result
                );
                break;
            case ResultType.Defense:
                ApplyToPlayerValue(
                    () => playerRuntime.Defense,
                    value => playerRuntime.Defense = value,
                    null,
                    result
                );
                break;
            case ResultType.CriticalRate:
                ApplyToPlayerValue(
                    () => playerRuntime.CriticalRate,
                    value => playerRuntime.CriticalRate = value,
                    null,
                    result
                );
                break;
            case ResultType.CriticalDamage:
                ApplyToPlayerValue(
                    () => playerRuntime.CriticalDamage,
                    value => playerRuntime.CriticalDamage = value,
                    null,
                    result
                );
                break;
            case ResultType.BlockRate:
                ApplyToPlayerValue(
                    () => playerRuntime.BlockRate,
                    value => playerRuntime.BlockRate = value,
                    null,
                    result
                );
                break;
            case ResultType.DodgeRate:
                ApplyToPlayerValue(
                    () => playerRuntime.DodgeRate,
                    value => playerRuntime.DodgeRate = value,
                    null,
                    result
                );
                break;
            case ResultType.Distance:
                ApplyToDistance(result);
                break;
            default:
                Debug.LogWarning($"未处理的结果类型：{result.type}");
                break;
        }
    }

    private static void ApplyToDistance(StatResult result)
    {
        if (RouteProgressManager.Instance == null)
        {
            return;
        }

        int current = RouteProgressManager.Instance.GetDistance();
        switch (result.operation)
        {
            case ResultOperation.Add:
                RouteProgressManager.Instance.Advance(Mathf.RoundToInt(result.value));
                break;
            case ResultOperation.Subtract:
                RouteProgressManager.Instance.Advance(-Mathf.RoundToInt(result.value));
                break;
            case ResultOperation.Set:
                Debug.LogWarning("Distance(Set) 未实现：当前 RouteProgressManager 只提供前进/休息接口");
                break;
            case ResultOperation.AddPercent:
                RouteProgressManager.Instance.Advance(Mathf.RoundToInt(current * result.value));
                break;
            case ResultOperation.SubtractPercent:
                RouteProgressManager.Instance.Advance(-Mathf.RoundToInt(current * result.value));
                break;
            case ResultOperation.SetPercent:
                Debug.LogWarning("Distance(SetPercent) 未实现：当前 RouteProgressManager 只提供前进/休息接口");
                break;
            case ResultOperation.AddMaxPercent:
            case ResultOperation.SubtractMaxPercent:
            case ResultOperation.SetMaxPercent:
                Debug.LogWarning("Distance 不支持 MaxPercent 操作（当前没有 Max 概念）");
                break;
        }
    }

    private static void ApplyToPlayerValue(
        Func<float> getter,
        Action<float> setter,
        float? maxValue,
        StatResult result
    )
    {
        if (getter == null || setter == null)
        {
            return;
        }

        float currentValue = getter();
        float baseForMax = maxValue.HasValue ? maxValue.Value : currentValue;
        float nextValue = currentValue;

        switch (result.operation)
        {
            case ResultOperation.Add:
                nextValue = currentValue + result.value;
                break;
            case ResultOperation.Subtract:
                nextValue = currentValue - result.value;
                break;
            case ResultOperation.Set:
                nextValue = result.value;
                break;
            case ResultOperation.AddPercent:
                nextValue = currentValue + currentValue * result.value;
                break;
            case ResultOperation.SubtractPercent:
                nextValue = currentValue - currentValue * result.value;
                break;
            case ResultOperation.SetPercent:
                nextValue = currentValue * result.value;
                break;
            case ResultOperation.AddMaxPercent:
                nextValue = currentValue + baseForMax * result.value;
                break;
            case ResultOperation.SubtractMaxPercent:
                nextValue = currentValue - baseForMax * result.value;
                break;
            case ResultOperation.SetMaxPercent:
                nextValue = baseForMax * result.value;
                break;
        }

        if (maxValue.HasValue)
        {
            nextValue = Mathf.Clamp(nextValue, 0f, Mathf.Max(0f, maxValue.Value));
        }

        setter(nextValue);
    }
}

[Serializable]
public class StatResult
{
    public ResultType type;    // 结果类型
    public ResultOperation operation; // 操作类型
    public float value;               // 数值变化量（正为获得，负为损失）
}

public enum ResultType
{
    HP,
    Health,
    Hunger,
    Energy,
    Money,
    Attack,
    Defense,
    CriticalRate,   // 暴击率（0~1）
    CriticalDamage, // 暴击伤害（百分比）
    BlockRate,      //格挡
    DodgeRate,      //闪避
    Distance,       // 增加/减少距离
}

// 操作类型枚举
public enum ResultOperation
{
    Add,               // 增加固定值
    Subtract,          // 减少固定值
    Set,               // 设为固定值
    SetPercent,        // 设为当前值的百分比（0~1）
    AddPercent,        // 增加当前值的百分比
    SubtractPercent,   // 减少当前值的百分比
    SetMaxPercent,     // 设为当前最大值的百分比（0~1）
    AddMaxPercent,     // 增加当前最大值的百分比
    SubtractMaxPercent // 减少当前最大值的百分比
}