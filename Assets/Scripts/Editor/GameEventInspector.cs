using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameEvent))]
public class GameEventInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("id"),
            new GUIContent("Id（事件编号）"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("eventName"),
            new GUIContent("Event Name（事件名称）"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("eventDescription"),
            new GUIContent("Event Description（事件描述）"));
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("condition"),
            new GUIContent("Condition（触发条件）"),
            true);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("options"),
            new GUIContent("Options（选项列表）"),
            true);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("defaultResults"),
            new GUIContent("Default Results（默认结果）"),
            true);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("weight"),
            new GUIContent("Weight（随机权重）"));

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(EventCondition))]
public class EventConditionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("extraPredicates"), true);
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(line, "Condition（触发条件）");

        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty extraProp = property.FindPropertyRelative("extraPredicates");
        float extraHeight = EditorGUI.GetPropertyHeight(extraProp, true);
        Rect extraRect = new Rect(line.x, line.y, line.width, extraHeight);
        EditorGUI.PropertyField(extraRect, extraProp, new GUIContent("Extra Predicates（扩展判定）"), true);

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(EventOption))]
public class EventOptionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("optionText"), true);
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("eventOutcomes"), true);
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(line, label.text);

        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty textProp = property.FindPropertyRelative("optionText");
        float textHeight = EditorGUI.GetPropertyHeight(textProp, true);
        Rect textRect = new Rect(line.x, line.y, line.width, textHeight);
        EditorGUI.PropertyField(textRect, textProp, new GUIContent("Option Text（选项文本）"), true);

        line.y += textHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty outcomesProp = property.FindPropertyRelative("eventOutcomes");
        float outcomesHeight = EditorGUI.GetPropertyHeight(outcomesProp, true);
        Rect outcomesRect = new Rect(line.x, line.y, line.width, outcomesHeight);
        EditorGUI.PropertyField(outcomesRect, outcomesProp, new GUIContent("Event Outcomes（选项结果组）"), true);

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(EventOutcome))]
public class EventOutcomeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("probability"), true);
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("results"), true);
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("outcomeDescription"), true);
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(line, label.text);

        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty probabilityProp = property.FindPropertyRelative("probability");
        float pHeight = EditorGUI.GetPropertyHeight(probabilityProp, true);
        Rect pRect = new Rect(line.x, line.y, line.width, pHeight);
        EditorGUI.PropertyField(pRect, probabilityProp, new GUIContent("Probability（概率）"), true);

        line.y += pHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty resultsProp = property.FindPropertyRelative("results");
        float rHeight = EditorGUI.GetPropertyHeight(resultsProp, true);
        Rect rRect = new Rect(line.x, line.y, line.width, rHeight);
        EditorGUI.PropertyField(rRect, resultsProp, new GUIContent("Results（结果列表）"), true);

        line.y += rHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty descProp = property.FindPropertyRelative("outcomeDescription");
        float dHeight = EditorGUI.GetPropertyHeight(descProp, true);
        Rect dRect = new Rect(line.x, line.y, line.width, dHeight);
        EditorGUI.PropertyField(dRect, descProp, new GUIContent("Outcome Description（结果描述）"), true);

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(EventResult))]
public class EventResultDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        h += EditorGUIUtility.standardVerticalSpacing;
        h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("resultType"), true);
        h += EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty typeProp = property.FindPropertyRelative("resultType");
        EventResultType type = (EventResultType)typeProp.enumValueIndex;
        if (type == EventResultType.Stat)
        {
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("statResult"), true);
        }
        else if (type == EventResultType.Item)
        {
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("itemResult"), true);
        }
        else
        {
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("enemyId"), true);
        }
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(line, label.text);

        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        SerializedProperty typeProp = property.FindPropertyRelative("resultType");
        float tHeight = EditorGUI.GetPropertyHeight(typeProp, true);
        Rect tRect = new Rect(line.x, line.y, line.width, tHeight);
        EditorGUI.PropertyField(tRect, typeProp, new GUIContent("Result Type（结果类型）"), true);

        line.y += tHeight + EditorGUIUtility.standardVerticalSpacing;
        EventResultType type = (EventResultType)typeProp.enumValueIndex;
        if (type == EventResultType.Stat)
        {
            SerializedProperty statProp = property.FindPropertyRelative("statResult");
            float sHeight = EditorGUI.GetPropertyHeight(statProp, true);
            Rect sRect = new Rect(line.x, line.y, line.width, sHeight);
            EditorGUI.PropertyField(sRect, statProp, new GUIContent("Stat Result（属性结果）"), true);
        }
        else if (type == EventResultType.Item)
        {
            SerializedProperty itemProp = property.FindPropertyRelative("itemResult");
            float iHeight = EditorGUI.GetPropertyHeight(itemProp, true);
            Rect iRect = new Rect(line.x, line.y, line.width, iHeight);
            EditorGUI.PropertyField(iRect, itemProp, new GUIContent("Item Result（物品结果）"), true);
        }
        else
        {
            SerializedProperty enemyIdProp = property.FindPropertyRelative("enemyId");
            float enemyHeight = EditorGUI.GetPropertyHeight(enemyIdProp, true);
            Rect enemyRect = new Rect(line.x, line.y, line.width, enemyHeight);
            EditorGUI.PropertyField(enemyRect, enemyIdProp, new GUIContent("Enemy Id（敌人ID）"), true);
        }

        EditorGUI.EndProperty();
    }
}
