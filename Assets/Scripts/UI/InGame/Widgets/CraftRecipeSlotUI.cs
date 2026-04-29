using UnityEngine;
using UnityEngine.UI;

public class CraftRecipeSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private ItemFrame material1Frame;
    [SerializeField] private ItemFrame material2Frame;
    [SerializeField] private GameObject plusSign;
    [SerializeField] private ItemFrame resultFrame;

    private CraftRecipe recipe;
    private System.Action<CraftRecipe> onSelected;

    /// <summary>
    /// 初始化按钮点击与“加号”引用。
    /// </summary>
    private void Awake()
    {
        if (plusSign == null)
        {
            // Prefab 中该对象通常命名为“加”
            var t = transform.Find("加");
            if (t != null) plusSign = t.gameObject;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelected?.Invoke(recipe));
        }
    }

    /// <summary>
    /// 绑定配方数据与选择回调，并刷新显示。
    /// </summary>
    public void Bind(CraftRecipe recipe, System.Action<CraftRecipe> onSelected)
    {
        this.recipe = recipe;
        this.onSelected = onSelected;
        Refresh();
    }

    /// <summary>
    /// 刷新材料/成品显示与缺料提示。
    /// </summary>
    public void Refresh()
    {
        if (recipe == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        ApplyMaterial(material1Frame, 0);
        ApplyMaterial(material2Frame, 1);
        RefreshPlusSign();

        if (resultFrame != null)
        {
            resultFrame.ClearCountOverride();
            if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGet(recipe.resultItemId, out ItemBase resultItem))
            {
                resultFrame.SetItem(resultItem);
            }
            resultFrame.SetCountText("x1");
            resultFrame.UpdateInfo();
        }
    }

    /// <summary>
    /// 刷新指定索引材料格（0/1），并根据库存设置数量文本与缺料红字。
    /// </summary>
    private void ApplyMaterial(ItemFrame frame, int index)
    {
        if (frame == null) return;

        frame.ClearCountOverride();

        if (recipe.materials == null || index >= recipe.materials.Count || recipe.materials[index] == null || recipe.materials[index].itemId <= 0)
        {
            frame.gameObject.SetActive(false);
            return;
        }

        var mat = recipe.materials[index];
        if (mat.count <= 0)
        {
            // 需求数量为 0（或负数）时视为无效材料，避免显示 “owned/0”
            frame.gameObject.SetActive(false);
            return;
        }

        frame.gameObject.SetActive(true);
        if (ItemRegistry.Instance != null && ItemRegistry.Instance.TryGet(mat.itemId, out ItemBase matItem))
        {
            frame.SetItem(matItem);
        }

        int owned = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(mat.itemId) : 0;
        bool enough = owned >= mat.count;
        frame.SetCountText($"{owned}/{mat.count}", enough ? (Color?)null : Color.red);
        frame.UpdateInfo();
    }

    /// <summary>
    /// 仅当两个材料格都显示时展示中间“+”号。
    /// </summary>
    private void RefreshPlusSign()
    {
        if (plusSign == null) return;

        bool showPlus = material1Frame != null && material1Frame.gameObject.activeSelf
                        && material2Frame != null && material2Frame.gameObject.activeSelf;
        plusSign.SetActive(showPlus);
    }
}

