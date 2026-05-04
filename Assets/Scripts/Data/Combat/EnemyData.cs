using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class BattleRewardEntry
{
    public enum RewardKind
    {
        None = 0,
        Item = 1,
        Money = 2
    }

    public RewardKind kind;
    public int itemId;
    public int itemCount;
    public float moneyYuan;
}

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

    [Header("战斗结算奖励")]
    [Tooltip("胜利时发放：物品或金钱条目；可为空。")]
    [SerializeField] private List<BattleRewardEntry> commonRewards = new List<BattleRewardEntry>();

    [Tooltip("胜利时发放：与常见奖励结构相同，用于「额外一次胜利奖励」等扩展；可为空。")]
    [SerializeField] private List<BattleRewardEntry> extraRewards = new List<BattleRewardEntry>();

    [Tooltip("胜利叙述补充句，如【你从女贼身上抢了0.4元】；可为空。")]
    [SerializeField] private string extraVictoryDescription;

    public string EnemyId => enemyId;
    public int Level => level;
    public bool CanEscape => canEscape;
    public string BattleOpeningTaunt => battleOpeningTaunt ?? string.Empty;

    public IReadOnlyList<EnemyBattleTraitAsset> BattleTraits => battleTraits;

    public IReadOnlyList<BattleRewardEntry> CommonRewards =>
        commonRewards != null ? commonRewards : Array.Empty<BattleRewardEntry>();

    public IReadOnlyList<BattleRewardEntry> ExtraRewards =>
        extraRewards != null ? extraRewards : Array.Empty<BattleRewardEntry>();

    public string ExtraVictoryDescription => extraVictoryDescription ?? string.Empty;
}
