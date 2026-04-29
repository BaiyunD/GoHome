using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftPage : MonoBehaviour
{
    [Header("List")]
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private CraftRecipeSlotUI recipeSlotPrefab;
    [SerializeField] private int recipesPerPage = 4;
    [SerializeField] private float slotVerticalSpacing = 170f;

    [Header("Details")]
    [SerializeField] private CraftDetailsPanel detailsPanel;

    [Header("Pagination")]
    [SerializeField] private TMP_Text pageText;

    private readonly List<CraftRecipeSlotUI> slots = new List<CraftRecipeSlotUI>();
    private CraftRecipe selectedRecipe;
    private int pageIndex;

    /// <summary>
    /// 关闭制作面板。
    /// </summary>
    public void OnClose()
    {
        UIManager.Instance.CloseUIEntry(UIKey.CraftPage);
    }

    public void OnOpenInventory()
    {
        UIManager.Instance.OpenUIEntry(UIKey.InventoryPage);
    }

    /// <summary>
    /// 打开时订阅数据变化并刷新列表。
    /// </summary>
    private void OnEnable()
    {
        if (CraftManager.Instance != null)
        {
            CraftManager.Instance.OnCrafted -= Refresh;
            CraftManager.Instance.OnCrafted += Refresh;
        }
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= Refresh;
            InventoryManager.Instance.OnItemChanged += Refresh;
        }
        Refresh();
    }

    /// <summary>
    /// 关闭时取消订阅，避免重复监听。
    /// </summary>
    private void OnDisable()
    {
        if (CraftManager.Instance != null) CraftManager.Instance.OnCrafted -= Refresh;
        if (InventoryManager.Instance != null) InventoryManager.Instance.OnItemChanged -= Refresh;
    }

    /// <summary>
    /// 制作面板显示入口（由 UIManager 调用）。
    /// </summary>
    public void Show()
    {
        Refresh();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏制作面板。
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 下一页（可绑定到“下一页”按钮）。
    /// </summary>
    public void NextPage()
    {
        pageIndex++;
        Refresh();
    }

    /// <summary>
    /// 上一页（可绑定到“上一页”按钮）。
    /// </summary>
    public void PrevPage()
    {
        pageIndex = Mathf.Max(0, pageIndex - 1);
        Refresh();
    }

    /// <summary>
    /// 刷新配方列表与详情面板。不使用 Find：全部依赖 Inspector 引用与预制体实例。
    /// </summary>
    private void Refresh()
    {
        var recipes = CraftManager.Instance != null ? CraftManager.Instance.GetAllRecipes() : null;
        int total = recipes != null ? recipes.Count : 0;

        if (recipesPerPage <= 0) recipesPerPage = 4;
        int maxPage = total == 0 ? 0 : (total - 1) / recipesPerPage;
        pageIndex = Mathf.Clamp(pageIndex, 0, maxPage);

        EnsureSlots(recipesPerPage);
        ApplyManualLayoutIfNeeded();

        for (int i = 0; i < slots.Count; i++)
        {
            int recipeIndex = pageIndex * recipesPerPage + i;
            CraftRecipe recipe = (recipes != null && recipeIndex < total) ? recipes[recipeIndex] : null;

            if (recipe == null)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            slots[i].Bind(recipe, SelectRecipe);
            slots[i].Refresh();
        }

        if (selectedRecipe == null && recipes != null && recipes.Count > 0)
        {
            SelectRecipe(recipes[0]);
        }
        else
        {
            detailsPanel?.ShowRecipe(selectedRecipe);
        }

        UpdatePageText(total, maxPage);
    }

    /// <summary>
    /// 如果列表容器没有挂任何 LayoutGroup，则手动把每个 slot 沿 Y 轴排开，避免全部重叠导致“看起来只有一个”。
    /// </summary>
    private void ApplyManualLayoutIfNeeded()
    {
        if (recipeListContainer == null) return;

        // 如果你在容器上挂了 VerticalLayoutGroup / GridLayoutGroup，就交给 Unity 布局系统。
        if (recipeListContainer.GetComponent<LayoutGroup>() != null) return;

        for (int i = 0; i < slots.Count; i++)
        {
            var rt = slots[i].transform as RectTransform;
            if (rt == null) continue;

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(0f, -i * slotVerticalSpacing);
        }
    }

    /// <summary>
    /// 确保列表中至少存在指定数量的 slot 实例。
    /// </summary>
    private void EnsureSlots(int count)
    {
        if (recipeListContainer == null || recipeSlotPrefab == null)
            return;

        while (slots.Count < count)
        {
            var slot = Instantiate(recipeSlotPrefab, recipeListContainer);
            slots.Add(slot);
        }
    }

    /// <summary>
    /// 刷新页码文本，格式为“当前页/总页数”。
    /// </summary>
    private void UpdatePageText(int totalRecipes, int maxPage)
    {
        if (pageText == null) return;

        if (totalRecipes <= 0)
        {
            pageText.text = "0/0";
            return;
        }

        int currentPage = pageIndex + 1;
        int totalPages = maxPage + 1;
        pageText.text = $"{currentPage}/{totalPages}";
    }

    /// <summary>
    /// 选中配方并刷新右侧详情区。
    /// </summary>
    private void SelectRecipe(CraftRecipe recipe)
    {
        selectedRecipe = recipe;
        if (detailsPanel != null) detailsPanel.ShowRecipe(recipe);
    }
}

