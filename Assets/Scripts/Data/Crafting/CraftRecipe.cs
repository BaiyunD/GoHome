using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "GoHome/Recipe")]
public class CraftRecipe : ScriptableObject
{
    /// <summary>
    /// 配方唯一标识（可用于存档/查表）。
    /// </summary>
    public int id;

    /// <summary>
    /// 配方显示名称；详情面板默认显示成品名称，此字段可用于列表标识或扩展。
    /// </summary>
    public string recipeName;

    [Tooltip("支持 1-2 种材料")]
    /// <summary>
    /// 制作所需材料列表。当前 UI 最多展示前 2 种有效材料。
    /// </summary>
    public List<CraftMaterial> materials;

    [Tooltip("只有一种成品")]
    /// <summary>
    /// 配方产出物品（固定 1 个）。
    /// </summary>
    public int resultItemId;

}

[System.Serializable]
public class CraftMaterial
{
    /// <summary>
    /// 材料物品引用。
    /// </summary>
    public int itemId;

    /// <summary>
    /// 本材料需求数量（小于等于 0 视为无效配置）。
    /// </summary>
    public int count;
}

