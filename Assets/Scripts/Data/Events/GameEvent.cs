using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


//TODO:完善事件系统，现在事件面板的两个按钮均不生效，要增加选择完选项后的内容输出，各种UI的开启关闭时间控制,完善结局系统
[CreateAssetMenu(fileName = "NewEvent", menuName = "GoHome/GameEvent")]
public class GameEvent : ScriptableObject
{
    [Header("基本信息")]
    [InspectorName("Id（事件编号）")]
    public int id;
    [InspectorName("Event Name（事件名称）")]
    public string eventName;
    [InspectorName("Event Description（事件描述）")]
    [TextArea] public string eventDescription;

    [Header("触发条件")]
    [InspectorName("Condition（触发条件）")]
    public EventCondition condition;   // 触发条件（如距离 <= 50）

    [Header("选项（自带结果，无需再设默认结果）")]
    [InspectorName("Options（选项列表）")]
    public List<EventOption> options;       // 选项列表

    [Header("默认结果（无选项时调用）")]
    [InspectorName("Default Results（默认结果）")]
    public List<EventResult> defaultResults;       // 事件可能产生的结果列表

    [Header("随机事件权重（0表示不会随机到）")]
    [InspectorName("Weight（随机权重）")]
    [Range(0, 100)]
    public int weight = 10;                 // 随机池中的权重

    // 运行时标记（固定事件是否已触发）
    [System.NonSerialized]
    public bool triggered = false;

    /// <summary>
    /// 异步应用结果（用于战斗、对话等需要等待的情况）
    /// </summary>
    //public IEnumerator ApplyResultsAsync()
    //{
    //    foreach (var result in results)
    //    {
    //        switch (result.type)
    //        {
    //            case ResultType.Battle:
    //                yield return StartCoroutine(BattleManager.Instance.StartBattle(result.value));
    //                break;
    //            case ResultType.Dialogue:
    //                yield return StartCoroutine(UIManager.Instance.ShowDialogue(result.dialogueId));
    //                break;
    //            default:
    //                // 同步结果直接应用
    //                ApplySingleResult(result);
    //                break;
    //        }
    //    }
    //}

    //private void ApplySingleResult(EventResult result)
    //{
    //    switch (result.type)
    //    {
    //        case ResultType.Money:
    //            GameManager.Instance.ModifyMoney(result.value);
    //            break;
    //        case ResultType.Health:
    //            GameManager.Instance.ModifyHealth(result.value);
    //            break;
    //        case ResultType.Hunger:
    //            GameManager.Instance.ModifyHunger(result.value);
    //            break;
    //        case ResultType.Energy:
    //            GameManager.Instance.ModifyEnergy(result.value);
    //            break;
    //        case ResultType.Distance:
    //            GameManager.Instance.ModifyDistance(result.value);
    //            break;
    //    }
    //}
}

[Serializable]
public class EventOption
{
    [InspectorName("Option Text（选项文本）")]
    public string optionText;               // 选项按钮上的文字
    [InspectorName("Event Outcomes（选项结果组）")]
    public List<EventOutcome> eventOutcomes;
}

[Serializable]
public class EventOutcome
{
    [InspectorName("Probability（概率）")]
    [Range(0, 100)]
    public float probability = 100;          // 概率百分比（0-100）
    [InspectorName("Results（结果列表）")]
    public List<EventResult> results;        // 该结果组触发的效果
    [InspectorName("Outcome Description（结果描述）")]
    [TextArea] public string outcomeDescription; // 结果描述文本（显示给玩家）
}

[Serializable]
public class EventResult
{
    [InspectorName("Result Type（结果类型）")]
    public EventResultType resultType = EventResultType.Stat;

    [InspectorName("Stat Result（属性结果）")]
    public StatResult statResult;
    [InspectorName("Item Result（物品结果）")]
    public ItemResult itemResult;
    [InspectorName("Enemy Id（敌人ID）")]
    public string enemyId;
}

public enum EventResultType
{
    Stat = 0,
    Item = 1,
    EnemyEncounter = 2
}

[Serializable]
public class ItemResult
{
    [InspectorName("Item ID（物品ID）")]
    public int itemID;           // 如果是物品，物品的ID
    [InspectorName("Count（数量）")]
    public int count;
}

[Serializable]
public class EventCondition
{
    [Header("扩展判定（可选）")]
    [InspectorName("Extra Predicates（扩展判定）")]
    public List<EventConditionPredicate> extraPredicates;

    public bool IsMet(int distance, int day, string currentRegionCode)
    {
        if (extraPredicates != null && extraPredicates.Count > 0)
        {
            int mainRegionId = ParseMainRegionId(currentRegionCode);
            EventConditionContext context = new EventConditionContext
            {
                Distance = distance,
                Day = day,
                RegionId = mainRegionId
            };

            for (int i = 0; i < extraPredicates.Count; i++)
            {
                EventConditionPredicate predicate = extraPredicates[i];
                if (predicate == null) continue;
                if (!predicate.IsMet(context)) return false;
            }
        }

        return true;
    }

    public static void ValidateRegionCodeOrThrow(string code, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(code) || !Regex.IsMatch(code, @"^\d+_\d+$"))
        {
            throw new InvalidOperationException(
                $"{fieldName} 格式非法，必须为 main_sub（数字_数字），当前值：{code}"
            );
        }
    }

    private static int ParseMainRegionId(string code)
    {
        ValidateRegionCodeOrThrow(code, "CurrentRegionCode");
        string[] parts = code.Split('_');
        return int.Parse(parts[0]);
    }
}