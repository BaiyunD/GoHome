using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "GoHome/EnemyData")]
public class EnemyData : CharacterDataBase
{
    [Header("敌人扩展")]
    [SerializeField] private string enemyId;
    [SerializeField] private int level = 1;
    [Header("敌人行为")]
    [SerializeField] private bool canEscape = false;
    [Tooltip("0~100，30 表示 30%")]
    [SerializeField] private float escapeRatePercent = 0f;
    public int expReward;
    public int moneyReward;

    // 战斗结果额外效果（可扩展）
    public List<BattleResultEffect> onWinEffects;
    public List<BattleResultEffect> onLoseEffects;
    public List<BattleResultEffect> onEscapeEffects;

    public string EnemyId => enemyId;
    public int Level => level;
    public bool CanEscape => canEscape;
    public float EscapeRatePercent => CharacterDataBase.ClampRate(escapeRatePercent);
}

[System.Serializable]
public class BattleResultEffect
{
    public ResultType type;      // 复用事件系统的结果类型
    public ResultOperation operation;
    public float value;
}
