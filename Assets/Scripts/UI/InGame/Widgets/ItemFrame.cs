using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemFrame : MonoBehaviour
{
    [SerializeField] Image itemIconImage;
    [SerializeField] Image hightLight;
    [SerializeField] TMP_Text itemCountText;

    private ItemBase item;
    private bool isSelected;

    private string overrideCountText;
    private Color? overrideCountColor;
    private Color defaultCountColor;
    private bool hasDefaultCountColor;

    private void Awake()
    {
        TryCacheDefaultCountColor();
    }

    private void TryCacheDefaultCountColor()
    {
        if (hasDefaultCountColor || itemCountText == null) return;

        defaultCountColor = itemCountText.color;
        // 有些预制体初始颜色可能是透明，兜底为白色，避免数量文本“存在但看不见”。
        if (defaultCountColor.a <= 0f) defaultCountColor = Color.white;
        hasDefaultCountColor = true;
    }

    /// <summary>
    /// 设置该格子显示的物品引用。
    /// </summary>
    /// <param name="item">要显示的物品</param>
    public void SetItem(ItemBase item)
    {
        this.item = item;
    }

    /// <summary>
    /// 覆写数量文本显示（用于制作面板显示“拥有/需要”等），不影响背包面板的默认计数逻辑。
    /// </summary>
    /// <param name="text">要显示的文本</param>
    /// <param name="color">可选：数量文本颜色（例如缺料时红色）</param>
    public void SetCountText(string text, Color? color = null)
    {
        TryCacheDefaultCountColor();
        overrideCountText = text;
        overrideCountColor = color;
        if (itemCountText != null)
        {
            itemCountText.text = overrideCountText;
            itemCountText.color = overrideCountColor.HasValue ? overrideCountColor.Value : defaultCountColor;
        }
    }

    /// <summary>
    /// 清除数量显示覆写，恢复为背包数量的默认显示逻辑。
    /// </summary>
    public void ClearCountOverride()
    {
        overrideCountText = null;
        overrideCountColor = null;
    }

    /// <summary>
    /// 设置选中状态（仅影响高亮显示与点击逻辑）。
    /// </summary>
    public void SetIsSelected(bool isSelected)
    {
        this.isSelected = isSelected;
    }

    public ItemBase GetItem()
    {
        return this.item;
    }

    /// <summary>
    /// 刷新图标与数量显示。
    /// 若存在覆写数量文本，则显示覆写内容；否则显示背包里该物品的数量。
    /// </summary>
    public void UpdateInfo()
    {
        TryCacheDefaultCountColor();
        if (item == null)
        {
            Debug.LogError("ItemFrame.UpdateInfo: item is null");
            return;
        }
        if (itemIconImage == null)
        {
            Debug.LogError("ItemFrame.UpdateInfo: itemIconImage is null");
            return;
        }
        if (itemCountText == null)
        {
            Debug.LogError("ItemFrame.UpdateInfo: itemCountText is null");
            return;
        }
        itemIconImage.sprite = item.Icon;
        if (!string.IsNullOrEmpty(overrideCountText))
        {
            itemCountText.text = overrideCountText;
            itemCountText.color = overrideCountColor.HasValue ? overrideCountColor.Value : defaultCountColor;
        }
        else
        {
            itemCountText.text = InventoryManager.Instance.GetItemCount(item.Id).ToString();
            itemCountText.color = defaultCountColor;
        }
    }

    public void OnClick()
    {
        if (InventoryPage.Instance != null)
        {
            InventoryPage.Instance.SetCurrentItemFrame(this);
        }
    }

    public void OnClick_Craft()
    {
        if (!isSelected)
        {
            SetIsSelected(true);
            IsSelected(isSelected);
            //InventoryPage.Instance.panelDetailsPanel.SetItem(item);
            //InventoryPage.Instance.panelDetailsPanel.OnItemFrameSelected();
        }
        else
        {
            SetIsSelected(false);
            IsSelected(isSelected);
        }
    }

    /// <summary>
    /// 获取当前选中状态。
    /// </summary>
    public bool GetIsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// 设置高亮显隐。
    /// </summary>
    /// <param name="s">true 显示高亮；false 隐藏</param>
    public void IsSelected(bool s)
    {
        hightLight.gameObject.SetActive(s);
    }
}
