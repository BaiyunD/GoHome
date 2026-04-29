using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDetailsPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private new TMP_Text name;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button useButton;
    [SerializeField] private TMP_Text useButtonText;

    private ItemBase item;

    private void Awake()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveListener(OnUseButtonClicked);
            useButton.onClick.AddListener(OnUseButtonClicked);
        }

        if (useButtonText != null)
        {
            // 按钮文字不应拦截点击事件，避免“按钮看得到但点不动”。
            useButtonText.raycastTarget = false;
        }

        RefreshUseButtonState();
    }

    public void SetItem(ItemBase item)
    {
        this.item = item;
    }

    public void OnItemFrameSelected()
    {
        if (item == null)
        {
            Debug.LogWarning("未设置item");
            RefreshUseButtonState();
            return;
        }
        icon.sprite = item.Icon;
        name.text = item.DisplayName;
        description.text = item.GetDescription(GetItemLevel(item));
        RefreshUseButtonState();
    }

    /// <summary>
    /// 直接展示一个物品信息（不依赖先 SetItem 再 OnItemFrameSelected 的流程）。
    /// 制作面板会用它来展示“成品 + 配方描述”。
    /// </summary>
    /// <param name="item">要展示的物品</param>
    /// <param name="overrideDescription">可选：覆盖描述文本（为空则显示物品自身 description）</param>
    public void ShowItem(ItemBase item, string overrideDescription = null)
    {
        if (item == null)
        {
            Debug.LogWarning("未设置item");
            return;
        }
        icon.sprite = item.Icon;
        name.text = item.DisplayName;
        description.text = string.IsNullOrWhiteSpace(overrideDescription)
            ? item.GetDescription(GetItemLevel(item))
            : overrideDescription;
    }

    public void OnUseButtonClicked()
    {
        if (item == null || item.Kind != ItemKind.Consumable)
        {
            RefreshUseButtonState();
            return;
        }

        if (!InventoryManager.Instance.HasItem(item.Id) || InventoryManager.Instance.GetItemCount(item.Id) == 0)
        {
            ResultToastModal.ShowShared(UITexts.ITEM_OUT_OF_STOCK);
            RefreshUseButtonState();
            return;
        }

        int usingItemId = item.Id;
        if (!ConsumableEffectDispatcher.TryApply(item, out string resultText))
        {
            ResultToastModal.ShowShared(UITexts.ITEM_EFFECT_NOT_AVAILABLE);
            RefreshUseButtonState();
            return;
        }

        InventoryManager.Instance.RemoveItem(usingItemId, 1);
        ResultToastModal.ShowShared(resultText);
        RefreshUseButtonState();
    }

    /// <summary>
    /// 兼容旧按钮事件名称，建议改绑到 OnUseButtonClicked。
    /// </summary>
    public void OnUsed()
    {
        OnUseButtonClicked();
    }

    public void ClearSelectionDisplay()
    {
        item = null;
        if (icon != null) icon.sprite = null;
        if (name != null) name.text = string.Empty;
        if (description != null) description.text = string.Empty;
        RefreshUseButtonState();
    }

    private void RefreshUseButtonState()
    {
        bool canUse = item != null && item.Kind == ItemKind.Consumable;

        if (useButton != null)
        {
            useButton.gameObject.SetActive(canUse);
        }

        if (useButtonText != null)
        {
            useButtonText.gameObject.SetActive(canUse);
            if (canUse)
            {
                useButtonText.text = UITexts.USE_BUTTON_TEXT;
            }
        }
    }

    private static int GetItemLevel(ItemBase itemBase)
    {
        if (itemBase == null || InventoryManager.Instance == null)
        {
            return 0;
        }

        return InventoryManager.Instance.GetItemCount(itemBase.Id);
    }
}

public static class ConsumableEffectDispatcher
{
    public static bool TryApply(ItemBase item, out string resultText)
    {
        resultText = string.Empty;
        if (item == null || item.Kind != ItemKind.Consumable)
        {
            return false;
        }

        if (ItemEffectDispatcher.OnUseItemEffect(item, out resultText, out ItemEffectSource source))
        {
            Debug.Log($"ConsumableEffectDispatcher.TryApply -> item={item.Id}, source={source}.");
            return true;
        }

        throw new System.InvalidOperationException(
            $"ConsumableEffectDispatcher.TryApply -> item={item.Id}({item.DisplayName}) 未命中新资产效果，请完成 Common/Specific 挂载。");
    }
}

