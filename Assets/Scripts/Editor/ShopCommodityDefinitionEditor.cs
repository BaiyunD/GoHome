using UnityEditor;

[CustomEditor(typeof(ShopCommodityDefinition))]
public class ShopCommodityDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty commodityIdProp = serializedObject.FindProperty("commodityId");
        SerializedProperty itemIdProp = serializedObject.FindProperty("itemId");
        SerializedProperty tradePermissionProp = serializedObject.FindProperty("tradePermission");
        SerializedProperty buyPriceProp = serializedObject.FindProperty("buyPrice");
        SerializedProperty sellPriceProp = serializedObject.FindProperty("sellPrice");
        SerializedProperty tradeCountProp = serializedObject.FindProperty("tradeCount");
        SerializedProperty isSellAllProp = serializedObject.FindProperty("isSellAll");
        SerializedProperty isPriceIncreaseOnBuyProp = serializedObject.FindProperty("isPriceIncreaseOnBuy");

        EditorGUILayout.PropertyField(commodityIdProp);
        EditorGUILayout.PropertyField(itemIdProp);
        EditorGUILayout.PropertyField(tradePermissionProp);

        ShopTradePermission permission = (ShopTradePermission)tradePermissionProp.enumValueIndex;
        if (permission == ShopTradePermission.BuyAndSell)
        {
            EditorGUILayout.PropertyField(buyPriceProp);
            EditorGUILayout.PropertyField(sellPriceProp);
            EditorGUILayout.PropertyField(tradeCountProp);
        }
        else if (permission == ShopTradePermission.BuyOnly)
        {
            EditorGUILayout.PropertyField(buyPriceProp);
            EditorGUILayout.PropertyField(tradeCountProp);
            EditorGUILayout.PropertyField(isPriceIncreaseOnBuyProp);
        }
        else
        {
            EditorGUILayout.PropertyField(sellPriceProp);
            EditorGUILayout.PropertyField(isSellAllProp);
            if (!isSellAllProp.boolValue)
            {
                EditorGUILayout.PropertyField(tradeCountProp);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
