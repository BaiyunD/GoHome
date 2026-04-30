using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShopLotteryRewardOptionData))]
public sealed class ShopLotteryRewardOptionDataDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty kindProp = property.FindPropertyRelative("kind");
        if (kindProp == null)
        {
            return lineHeight + spacing + EditorGUI.GetPropertyHeight(property, label, true);
        }

        ShopLotteryRewardKind kind = (ShopLotteryRewardKind)kindProp.enumValueIndex;
        int extraLines = kind == ShopLotteryRewardKind.Item ? 2 : 1;

        return lineHeight + extraLines * (lineHeight + spacing);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect line = new Rect(position.x, position.y, position.width, lineHeight);
        SerializedProperty kindProp = property.FindPropertyRelative("kind");
        if (kindProp != null)
        {
            EditorGUI.PropertyField(line, kindProp);
        }

        line.y += lineHeight + spacing;

        ShopLotteryRewardKind kind = kindProp != null
            ? (ShopLotteryRewardKind)kindProp.enumValueIndex
            : ShopLotteryRewardKind.Item;

        if (kind == ShopLotteryRewardKind.Item)
        {
            SerializedProperty itemIdProp = property.FindPropertyRelative("itemId");
            SerializedProperty itemCountProp = property.FindPropertyRelative("itemCount");

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(line, itemIdProp);
            line.y += lineHeight + spacing;
            EditorGUI.PropertyField(line, itemCountProp);
            EditorGUI.indentLevel--;
        }
        else
        {
            SerializedProperty moneyProp = property.FindPropertyRelative("moneyAmount");
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(line, moneyProp);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}
