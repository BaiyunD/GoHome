using System;
using UnityEngine;

public class Stat
{
    public event Action OnChangedEvent;  // 数值变化时触发

    [SerializeField] private float currentValue;
    [SerializeField] private float maxValue;
    private bool haveMax;

    public Stat(float start = 0)
    {
        haveMax = false;
        currentValue = start;
    }

    public Stat(float max, float start = -1)
    {
        haveMax = true;
        maxValue = max;
        currentValue = start >= 0 ? start : max;
    }

    public int IntValue
    {
        get => Mathf.RoundToInt(currentValue);
    }

    public float Value
    {
        get => currentValue;
        set
        {
            float newValue = value;
            if (haveMax)
                newValue = Mathf.Clamp(value, 0, maxValue);

            if (Mathf.Approximately(currentValue, newValue)) return;//比较浮点数
            currentValue = newValue;
            OnChangedEvent?.Invoke();
        }
    }

    public float Max
    {
        get => maxValue;
        set
        {
            if (!haveMax) return;
            maxValue = value;
            if (currentValue > maxValue) Value = maxValue; // 限制当前值
        }
    }

    public void Add(float delta) => Value += delta;
    public void MaxAdd(float delta) => Max += delta;
    public void Subtract(float delta) => Value -= delta;
    public void Set(float newValue) => Value = newValue;
    public void SetPercent(float percent)
    {
        Value *= percent;
    }
    public void SetMaxPercent(float percent)
    {
        if (haveMax)
            Value = maxValue * percent;
        else
            Debug.LogWarning("没有最大值的属性无法设置最大值百分比");
    }

    public void Reset(float start = 0)
    {
        currentValue = start;
    }
    public void Reset(float max, float start = -1)
    {
        maxValue = max;
        currentValue = start >= 0 ? start : max;
    }
}
