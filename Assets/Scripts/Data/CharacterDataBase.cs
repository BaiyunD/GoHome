using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class CharacterDataBase : ScriptableObject
{
    [Header("基础信息")]
    [FormerlySerializedAs("enemyName")]
    [SerializeField] private string characterName;

    [Header("战斗属性（基础值）")]
    [FormerlySerializedAs("hp")]
    [SerializeField] private int hp = 100;
    [FormerlySerializedAs("attack")]
    [SerializeField] private int attack = 10;
    [FormerlySerializedAs("defense")]
    [SerializeField] private int defense = 0;

    [Tooltip("0~100，25 表示 25%")]
    [SerializeField] private float criticalRate = 0f;
    [Tooltip("最终倍率百分比：150 表示 150% 伤害")]
    [SerializeField] private float criticalDamage = 150f;
    [Tooltip("0~100，25 表示 25%")]
    [SerializeField] private float dodgeRate = 0f;
    [Tooltip("0~100，25 表示 25%")]
    [SerializeField] private float blockRate = 0f;
    [Tooltip("0~100，25 表示 25%")]
    [SerializeField] private float escapeRate = 50f;

    [Header("特性编号列表")]
    [SerializeField] private List<string> traitIds = new List<string>();

    public string CharacterName => characterName;
    public int HP => hp;
    public int Attack => attack;
    public int Defense => defense;

    public float CriticalRate => criticalRate;
    public float CriticalDamage => criticalDamage;
    public float DodgeRate => dodgeRate;
    public float BlockRate => blockRate;
    public float EscapeRate => escapeRate;

    public IReadOnlyList<string> TraitIds => traitIds;

    public static float ClampRate(float value)
    {
        return Mathf.Clamp(value, 0f, 100f);
    }
}
