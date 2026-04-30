using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShopLotteryPoolDefinition))]
public class ShopLotteryPoolDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty tiersProp = serializedObject.FindProperty("tiers");
        EditorGUILayout.PropertyField(tiersProp, true);

        ShopLotteryPoolDefinition pool = (ShopLotteryPoolDefinition)target;
        float sumP = pool.SumTierWeights();
        float thanksRemainder = Mathf.Max(0f, 100f - sumP);

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("概率汇总（运行时）", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"各档位权重之和 P = {sumP:0.###}%", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField($"隐式「谢谢惠顾」余量 = {thanksRemainder:0.###}%（P < 100 时生效）", EditorStyles.wordWrappedLabel);

        if (sumP > 100.001f)
        {
            EditorGUILayout.HelpBox("各档位权重之和超过 100%，抽奖在运行时将无法执行，请调整后再试。", MessageType.Warning);
        }
        else if (thanksRemainder > 0.001f && HasReservedThanksTier(pool))
        {
            EditorGUILayout.HelpBox(
                "存在隐式「谢谢惠顾」余量时，不应再使用档位名称「谢谢惠顾」。请将档位权重之和设为 100% 或修改该档位名称。",
                MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static bool HasReservedThanksTier(ShopLotteryPoolDefinition pool)
    {
        if (pool.Tiers == null)
        {
            return false;
        }

        for (int i = 0; i < pool.Tiers.Count; i++)
        {
            ShopLotteryTierData tier = pool.Tiers[i];
            if (tier == null)
            {
                continue;
            }

            string name = tier.DisplayName ?? string.Empty;
            if (string.Equals(name.Trim(), ShopLotteryPoolDefinition.ReservedThanksDisplayName, System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
