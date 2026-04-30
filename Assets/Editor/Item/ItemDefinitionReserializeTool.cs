using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ItemDefinitionReserializeTool
{
    private const string ITEM_DEFINITION_FOLDER = "Assets/Resources/ItemDefinitions";

    [MenuItem("Tools/Items/Cleanup ItemDefinition Legacy Fields")]
    public static void CleanupLegacyFields()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition", new[] { ITEM_DEFINITION_FOLDER });
        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning($"未找到任何 ItemDefinition：{ITEM_DEFINITION_FOLDER}");
            return;
        }

        List<string> assetPaths = new List<string>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (definition == null)
            {
                continue;
            }

            EditorUtility.SetDirty(definition);
            assetPaths.Add(path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.ForceReserializeAssets(assetPaths);

        Debug.Log($"ItemDefinition 重序列化完成，共处理 {assetPaths.Count} 个资源。");
    }
}