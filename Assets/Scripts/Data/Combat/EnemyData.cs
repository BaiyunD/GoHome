using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "GoHome/Enemy/EnemyData")]
public class EnemyData : CharacterDataBase
{
    [Header("敌人扩展")]
    [SerializeField] private string enemyId;
    [SerializeField] private int level = 1;

    [Header("战斗特性（资产）")]
    [Tooltip("按顺序执行；与 traitIds 无强制关联，战斗内以此列表为准。")]
    [SerializeField] private List<EnemyBattleTraitAsset> battleTraits = new List<EnemyBattleTraitAsset>();
    [Header("开战叙述")]
    [Tooltip("开战时在事件叙述弹窗显示；玩家首次操作（普攻或逃跑）后关闭并清空。可为空。")]
    [SerializeField] private string battleOpeningTaunt;

    [Header("敌人行为")]
    [SerializeField] private bool canEscape = false;
    public int expReward;
    public int moneyReward;

    // 战斗结果额外效果（可扩展）
    public List<BattleResultEffect> onWinEffects;
    public List<BattleResultEffect> onLoseEffects;
    public List<BattleResultEffect> onEscapeEffects;

    public string EnemyId => enemyId;
    public int Level => level;
    public bool CanEscape => canEscape;
    public string BattleOpeningTaunt => battleOpeningTaunt ?? string.Empty;

    public IReadOnlyList<EnemyBattleTraitAsset> BattleTraits => battleTraits;
}

[System.Serializable]
public class BattleResultEffect
{
    public ResultType type;      // 复用事件系统的结果类型
    public ResultOperation operation;
    public float value;
}
