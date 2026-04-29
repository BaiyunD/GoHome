using System.Text;
using TMPro;
using UnityEngine;

public class TraitsPage : MonoBehaviour
{
    [SerializeField] private TMP_Text bodyText;

    /// <summary>
    /// 关闭特性面板。
    /// </summary>
    public void OnClose()
    {
        UIManager.Instance.CloseUIEntry(UIKey.TraitsPage);
    }

    /// <summary>
    /// 由 UIManager 打开时调用。
    /// </summary>
    public void Show()
    {
        Refresh();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新特性列表展示（按定义字典中 TraitId 排序）。
    /// </summary>
    public void Refresh()
    {
        if (bodyText == null)
        {
            return;
        }

        if (TraitManager.Instance == null)
        {
            bodyText.text = string.Empty;
            return;
        }

        var sb = new StringBuilder();
        bool first = true;
        foreach (TraitDefinition d in TraitManager.Instance.GetAllDefinitionsSorted())
        {
            if (!first)
            {
                sb.Append("\n\n----------\n\n");
            }

            first = false;
            sb.Append("【特性名称】：");
            sb.Append(FormatField(d != null ? d.DisplayName : null));
            sb.Append('\n');
            sb.Append("【特性描述】：");
            sb.Append(FormatField(d != null ? d.Description : null));
        }

        bodyText.text = sb.ToString();
    }

    private static string FormatField(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "null";
        }

        return value;
    }
}

