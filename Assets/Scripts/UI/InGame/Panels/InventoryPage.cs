using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryPage : MonoBehaviour
{
    public static InventoryPage Instance {  get; private set; }

    [SerializeField] GameObject itemFramePF;
    [SerializeField] Transform itemFrameContainer;
    [FormerlySerializedAs("paneletailsPanel")]
    [SerializeField] public InventoryDetailsPanel panelDetailsPanel;

    [Header("背包分类Tab（纯绑定）")]
    [SerializeField] private Button tabAllButton;
    [SerializeField] private Image tabAllBgImage;
    [SerializeField] private TMP_Text tabAllLabel;

    [SerializeField] private Button tabMaterialButton;
    [SerializeField] private Image tabMaterialBgImage;
    [SerializeField] private TMP_Text tabMaterialLabel;

    [SerializeField] private Button tabConsumableButton;
    [SerializeField] private Image tabConsumableBgImage;
    [SerializeField] private TMP_Text tabConsumableLabel;

    [SerializeField] private Button tabToolButton;
    [SerializeField] private Image tabToolBgImage;
    [SerializeField] private TMP_Text tabToolLabel;

    [SerializeField] private Button tabSpecialItemButton;
    [SerializeField] private Image tabSpecialItemBgImage;
    [SerializeField] private TMP_Text tabSpecialItemLabel;

    private ItemKind? currentCategory = null; // null 表示“全部”
    private bool categoryTabsInitialized = false;

    private class CategoryTab
    {
        public ItemKind? Type;
        public Button Button;
        public Image BgImage;
        public TMP_Text TmpLabel;
        public float BaseAlpha;
    }

    private readonly List<CategoryTab> categoryTabs = new List<CategoryTab>();

    private readonly List<ItemFrame> itemFrames = new List<ItemFrame>();
    private ItemFrame currentItemFrame = null;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        InitializeCategoryUI();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= UpdateUIDisplay;
            InventoryManager.Instance.OnItemChanged += UpdateUIDisplay;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemChanged -= UpdateUIDisplay;
        }
    }

    private void ClearChild(Transform tf)
    {
        if (tf == null) return;
        for (int i = 0; i < tf.childCount; i++)
        {
            if (tf.GetChild(i))
            {
                Destroy(tf.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// 点击关闭按钮
    /// </summary>
    public void OnClose()
    {
        UIManager.Instance.CloseUIEntry(UIKey.InventoryPage);
    }

    public void OnOpenCraft()
    {
        UIManager.Instance.OpenUIEntry(UIKey.CraftPage);
    }

    public void SetCurrentItemFrame(ItemFrame itemFrame)
    {
        if (itemFrame == null) return;

        if (currentItemFrame != null && currentItemFrame != itemFrame)
        {
            currentItemFrame.SetIsSelected(false);
            currentItemFrame.IsSelected(false);
        }

        currentItemFrame = itemFrame;
        currentItemFrame.SetIsSelected(true);
        currentItemFrame.IsSelected(true);

        if (panelDetailsPanel != null)
        {
            panelDetailsPanel.SetItem(currentItemFrame.GetItem());
            panelDetailsPanel.OnItemFrameSelected();
        }
    }

    public void UpdateUIDisplay()
    {
        ItemKind? filterKind = currentCategory;

        int previousSelectedItemId = currentItemFrame != null && currentItemFrame.GetItem() != null
            ? currentItemFrame.GetItem().Id
            : -1;

        ClearChild(itemFrameContainer);
        itemFrames.Clear();
        currentItemFrame = null;

        if (ItemRegistry.Instance == null)
        {
            Debug.LogWarning("InventoryPage.UpdateUIDisplay -> ItemRegistry.Instance is null");
            return;
        }

        foreach (int index in InventoryManager.inventoryDict.Keys)
        {
            if (!ItemRegistry.Instance.TryGet(index, out ItemBase item)) continue;
            if (item == null) continue;
            if (filterKind.HasValue && item.Kind != filterKind.Value) continue;

            ItemFrame frame = Instantiate(itemFramePF, itemFrameContainer).GetComponent<ItemFrame>();
            frame.SetItem(item);
            frame.SetIsSelected(false);
            frame.IsSelected(false);
            frame.UpdateInfo();
            itemFrames.Add(frame);
        }

        if (itemFrames.Count > 0)
        {
            ItemFrame targetFrame = itemFrames[0];
            if (previousSelectedItemId > 0)
            {
                for (int i = 0; i < itemFrames.Count; i++)
                {
                    ItemBase candidate = itemFrames[i].GetItem();
                    if (candidate != null && candidate.Id == previousSelectedItemId)
                    {
                        targetFrame = itemFrames[i];
                        break;
                    }
                }
            }

            SetCurrentItemFrame(targetFrame);
        }
        else if (panelDetailsPanel != null)
        {
            panelDetailsPanel.ClearSelectionDisplay();
        }
    }

    public void Show()
    {
        InitializeCategoryUI();
        UpdateUIDisplay();
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void InitializeCategoryUI()
    {
        if (categoryTabsInitialized) return;

        if (transform == null) return;

        if (TryInitializeCategoryUIByBindings())
        {
            currentCategory = null;
            UpdateTabHighlight();
            categoryTabsInitialized = true;
            return;
        }
    }

    private bool TryInitializeCategoryUIByBindings()
    {
        // 任意一个按钮被绑定就认为进入“纯绑定模式”
        bool hasAnyBinding =
            tabAllButton != null ||
            tabMaterialButton != null ||
            tabConsumableButton != null ||
            tabToolButton != null ||
            tabSpecialItemButton != null;

        if (!hasAnyBinding) return false;

        categoryTabs.Clear();

        // 全部
        RegisterTabBinding(null, tabAllButton, tabAllBgImage, tabAllLabel, "全部");
        // 材料
        RegisterTabBinding(ItemKind.Material, tabMaterialButton, tabMaterialBgImage, tabMaterialLabel, "材料");
        // 消耗品
        RegisterTabBinding(ItemKind.Consumable, tabConsumableButton, tabConsumableBgImage, tabConsumableLabel, "消耗品");
        // 道具
        RegisterTabBinding(ItemKind.Tool, tabToolButton, tabToolBgImage, tabToolLabel, "道具");
        // 特殊物品
        RegisterTabBinding(ItemKind.Special, tabSpecialItemButton, tabSpecialItemBgImage, tabSpecialItemLabel, "特殊物品");

        return categoryTabs.Count > 0;
    }

    private void RegisterTabBinding(
        ItemKind? type,
        Button button,
        Image bgImage,
        TMP_Text label,
        string labelText)
    {
        if (button == null) return;

        Image effectiveBgImage = bgImage;
        if (effectiveBgImage == null)
        {
            effectiveBgImage = button.targetGraphic as Image;
        }

        if (effectiveBgImage == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { SetCategoryAndRefresh(type); });

        if (label != null && !string.IsNullOrWhiteSpace(labelText))
        {
            label.text = labelText;
        }

        float baseAlpha = effectiveBgImage.color.a;
        categoryTabs.Add(new CategoryTab()
        {
            Type = type,
            Button = button,
            BgImage = effectiveBgImage,
            TmpLabel = label,
            BaseAlpha = baseAlpha
        });
    }

    private void SetCategoryAndRefresh(ItemKind? category)
    {
        currentCategory = category;
        UpdateTabHighlight();
        UpdateUIDisplay();
    }

    private void UpdateTabHighlight()
    {
        for (int i = 0; i < categoryTabs.Count; i++)
        {
            CategoryTab tab = categoryTabs[i];
            if (tab == null || tab.BgImage == null) continue;

            bool selected = false;
            if (!tab.Type.HasValue && !currentCategory.HasValue)
            {
                selected = true;
            }
            else if (tab.Type.HasValue && currentCategory.HasValue && tab.Type.Value == currentCategory.Value)
            {
                selected = true;
            }

            Color c = tab.BgImage.color;
            float alphaMultiplier = selected ? 1f : 0.65f;
            tab.BgImage.color = new Color(c.r, c.g, c.b, tab.BaseAlpha * alphaMultiplier);
        }
    }
}

