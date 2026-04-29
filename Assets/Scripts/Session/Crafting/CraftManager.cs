using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour
{
    /// <summary>
    /// 全局制作管理器单例。
    /// </summary>
    public static CraftManager Instance { get; private set; }

    [Header("配方列表（自动读取）")]
    [SerializeField] private List<CraftRecipe> recipes = new List<CraftRecipe>();
    [SerializeField] private string recipeResourcesFolder = "CraftRecipes";
    [SerializeField] private bool autoLoadRecipesOnAwake = true;

    /// <summary>
    /// 制作成功后触发（扣料并添加成品完成后）。
    /// </summary>
    public event System.Action OnCrafted;

    /// <summary>
    /// 单例初始化。
    /// 注意：当前项目中多个 Manager 都采用场景中挂载 + Awake 赋值的方式。
    /// </summary>
    private void Awake()
    {
        Instance = this;
        if (autoLoadRecipesOnAwake)
        {
            ReloadRecipesFromResources();
        }
    }

    /// <summary>
    /// 获取所有可用配方列表（只读视图）。
    /// </summary>
    public IReadOnlyList<CraftRecipe> GetAllRecipes()
    {
        return recipes;
    }

    /// <summary>
    /// 从 Resources 指定目录自动读取全部 CraftRecipe。
    /// 例如目录填写 "CraftRecipes"，对应资源路径为 Assets/Resources/CraftRecipes。
    /// </summary>
    public void ReloadRecipesFromResources()
    {
        string folder = string.IsNullOrWhiteSpace(recipeResourcesFolder) ? string.Empty : recipeResourcesFolder.Trim();
        CraftRecipe[] loadedRecipes = Resources.LoadAll<CraftRecipe>(folder);

        recipes.Clear();
        if (loadedRecipes != null && loadedRecipes.Length > 0)
        {
            for (int i = 0; i < loadedRecipes.Length; i++)
            {
                if (loadedRecipes[i] != null)
                {
                    recipes.Add(loadedRecipes[i]);
                }
            }
        }

        recipes.Sort((a, b) =>
        {
            int idCompare = a.id.CompareTo(b.id);
            if (idCompare != 0) return idCompare;
            return string.Compare(a.name, b.name, System.StringComparison.Ordinal);
        });

        if (recipes.Count == 0)
        {
            Debug.LogWarning($"CraftManager: 在 Resources/{folder} 下未找到任何 CraftRecipe。", this);
        }
    }

    /// <summary>
    /// 检查是否满足制作条件（材料数量、配方合法性）。
    /// </summary>
    /// <param name="recipe">要检查的配方</param>
    /// <returns>材料满足且配方数据合法时返回 true</returns>
    public bool CanCraft(CraftRecipe recipe)
    {
        if (recipe == null) return false;
        if (recipe.resultItemId <= 0) return false;
        if (recipe.materials == null || recipe.materials.Count == 0) return false;
        if (recipe.materials.Count > 2) return false;
        if (InventoryManager.Instance == null) return false;

        int validMaterialCount = 0;
        foreach (var mat in recipe.materials)
        {
            if (mat == null || mat.itemId <= 0 || mat.count <= 0)
            {
                // 与制作面板显示逻辑保持一致：无效材料槽位视为“未配置”，直接忽略。
                continue;
            }

            validMaterialCount++;
            if (InventoryManager.Instance.GetItemCount(mat.itemId) < mat.count) return false;
        }

        // 至少要有一种有效材料，避免空配方也可制作。
        return validMaterialCount > 0;
    }

    /// <summary>
    /// 执行制作：扣除材料、添加成品。道具效果由被动系统结算。
    /// </summary>
    /// <param name="recipe">要制作的配方</param>
    /// <returns>制作成功返回 true；材料不足或配方不合法返回 false</returns>
    public bool Craft(CraftRecipe recipe)
    {
        if (!CanCraft(recipe)) return false;

        foreach (var mat in recipe.materials)
        {
            if (mat == null || mat.itemId <= 0 || mat.count <= 0) continue;
            InventoryManager.Instance.RemoveItem(mat.itemId, mat.count);
        }

        InventoryManager.Instance.AddItem(recipe.resultItemId, 1);

        OnCrafted?.Invoke();
        return true;
    }
}
