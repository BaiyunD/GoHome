using UnityEngine;

public static class MoneyUtil
{
    public static int YuanToCents(float yuan)
    {
        return Mathf.RoundToInt(yuan * 100f);
    }

    public static float CentsToYuan(int cents)
    {
        return cents / 100f;
    }

    public static int ClampNonNegativeCents(int cents)
    {
        return Mathf.Max(0, cents);
    }
}
