public static class UITexts
{
    public const string ITEM_OUT_OF_STOCK = "你已经用完了>~<";
    public const string ITEM_EFFECT_NOT_AVAILABLE = "该物品效果暂不可用";
    public const string CRAFT_NOT_ENOUGH_MATERIAL = "您没有足够材料";
    public const string USE_BUTTON_TEXT = "使用";

    public static string FormatCraftSuccess(string itemName)
    {
        return $"{itemName}+1";
    }

    public static string FormatUseWithoutEffect(string itemName)
    {
        return $"已使用1个{itemName}（暂未接入效果）";
    }

    public static string FormatUseSuccess(string itemName)
    {
        return $"已使用1个{itemName}";
    }
}

