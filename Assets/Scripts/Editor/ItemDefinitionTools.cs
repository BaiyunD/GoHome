using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemDefinitionTools : EditorWindow
{
    private const string RESOURCES_FOLDER = "Assets/Resources/ItemDefinitions";
    private const string RESOURCES_LOAD_PATH = "ItemDefinitions";

    [MenuItem("Tools/Items/ItemDefinition Tools")]
    public static void Open()
    {
        GetWindow<ItemDefinitionTools>("ItemDefinition Tools");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("ItemDefinition（SO）工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (GUILayout.Button("1) 分段自动分配ID（001/101/201/301）"))
        {
            AssignIdsBySegment();
        }

        EditorGUILayout.HelpBox(
            "说明：\n" +
            "- ItemDefinition 默认放在 Assets/Resources/ItemDefinitions（Resources 路径为 ItemDefinitions）\n" +
            "- 工具仅负责分段自动分配ID：材料001+，消耗品101+，道具201+，特殊301+",
            MessageType.Info);
    }

    private static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(RESOURCES_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "ItemDefinitions");
        }
    }

    private static void AssignIdsBySegment()
    {
        EnsureResourcesFolder();

        ItemDefinition[] defs = Resources.LoadAll<ItemDefinition>(RESOURCES_LOAD_PATH);
        List<ItemDefinition> list = new List<ItemDefinition>();
        if (defs != null) list.AddRange(defs);

        // 也把非 Resources 下的 ItemDefinition 找出来（避免用户放错目录）
        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            ItemDefinition def = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (def != null && !list.Contains(def))
            {
                list.Add(def);
            }
        }

        HashSet<int> used = new HashSet<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && list[i].Id > 0)
            {
                used.Add(list[i].Id);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            ItemDefinition def = list[i];
            if (def == null) continue;
            if (def.Id > 0) continue;

            int start = GetSegmentStart(def.Kind);
            int next = FindNextAvailableId(start, used);

            Undo.RecordObject(def, "Assign ItemDefinition Id");
            def.EditorSetId(next);
            EditorUtility.SetDirty(def);
            used.Add(next);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ItemDefinitionTools: 分段ID分配完成。");
    }

    private static int GetSegmentStart(ItemKind kind)
    {
        switch (kind)
        {
            case ItemKind.Material:
                return 1;   // 001
            case ItemKind.Consumable:
                return 101; // 101
            case ItemKind.Tool:
                return 201; // 201
            case ItemKind.Special:
                return 301; // 301
            default:
                return 1;
        }
    }

    private static int FindNextAvailableId(int start, HashSet<int> used)
    {
        int id = start;
        while (used.Contains(id))
        {
            id++;
        }
        return id;
    }
}

