using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftDetailsPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button craftButton;

    private CraftRecipe currentRecipe;

    /// <summary>
    /// 绑定制作按钮点击回调，并检查结果提示框引用是否已配置。
    /// </summary>
    private void Awake()
    {
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftClicked);
        }

    }

    /// <summary>
    /// 订阅制作与背包变化事件，保证按钮状态与数据同步。
    /// </summary>
    private void OnEnable()
    {
        if (CraftManager.Instance != null)
        {
            CraftManager.Instance.OnCrafted -= RefreshCraftButtonState;
            CraftManager.Instance.OnCrafted += RefreshCraftButtonState;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= RefreshCraftButtonState;
            InventoryManager.Instance.OnItemChanged += RefreshCraftButtonState;
        }
    }

    /// <summary>
    /// 取消事件订阅并在面板关闭时隐藏制作结果提示。
    /// </summary>
    private void OnDisable()
    {
        if (CraftManager.Instance != null) CraftManager.Instance.OnCrafted -= RefreshCraftButtonState;
        if (InventoryManager.Instance != null) InventoryManager.Instance.OnItemChanged -= RefreshCraftButtonState;
    }

    /// <summary>
    /// 绑定并展示一个配方的详情（成品图标/名称/描述），并刷新制作按钮是否可用。
    /// </summary>
    public void ShowRecipe(CraftRecipe recipe)
    {
        currentRecipe = recipe;

        if (recipe == null || recipe.resultItemId <= 0 || ItemRegistry.Instance == null)
        {
            if (icon != null) icon.sprite = null;
            if (title != null) title.text = string.Empty;
            if (description != null) description.text = string.Empty;
            RefreshCraftButtonState();
            return;
        }

        if (ItemRegistry.Instance.TryGet(recipe.resultItemId, out ItemBase resultItem) && resultItem != null)
        {
            if (icon != null) icon.sprite = resultItem.Icon;
            if (title != null) title.text = resultItem.DisplayName;
            if (description != null) description.text = resultItem.GetDescription(InventoryManager.Instance != null
                ? InventoryManager.Instance.GetItemCount(resultItem.Id)
                : 0);
        }

        RefreshCraftButtonState();
    }

    /// <summary>
    /// 根据当前是否选中有效配方设置制作按钮可用状态。
    /// </summary>
    public void RefreshCraftButtonState()
    {
        if (craftButton == null)
            return;

        craftButton.interactable = currentRecipe != null;
    }

    /// <summary>
    /// 点击制作：尝试执行配方，并显示成功/失败提示。
    /// </summary>
    private void OnCraftClicked()
    {
        if (CraftManager.Instance == null || currentRecipe == null)
            return;

        bool success = CraftManager.Instance.Craft(currentRecipe);
        if (success && ItemRegistry.Instance != null
            && ItemRegistry.Instance.TryGet(currentRecipe.resultItemId, out ItemBase resultItem)
            && resultItem != null)
        {
            ResultToastModal.ShowShared(UITexts.FormatCraftSuccess(resultItem.DisplayName));
        }
        else
        {
            ResultToastModal.ShowShared(UITexts.CRAFT_NOT_ENOUGH_MATERIAL);
        }

        RefreshCraftButtonState();
    }
}

