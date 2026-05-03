using System;
using UnityEngine;

/// <summary>单条效果：执行数值 + 参与通用日志拼装（可覆盖文案）。</summary>
[Serializable]
public sealed class EnemyTraitEffectLine
{
    [SerializeField] private EnemyTraitEffectKind kind = EnemyTraitEffectKind.None;

    [Tooltip("整型参：伤害/治疗/攻防增减等，按 Kind 解释；带符号时表示上升/下降。")]
    [SerializeField] private int intValue;

    [Tooltip("浮点参：率类增减（百分点或暴击伤害数值），按 Kind 解释。")]
    [SerializeField] private float floatValue;

    [Tooltip("非空时跳过模板，直接作为该条在通用段中的一句（不含首尾逗号）。")]
    [SerializeField] private string customNarrationFragment = string.Empty;

    public EnemyTraitEffectKind Kind => kind;
    public int IntValue => intValue;
    public float FloatValue => floatValue;
    public string CustomNarrationFragment => customNarrationFragment ?? string.Empty;
}
