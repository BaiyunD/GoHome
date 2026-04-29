using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("地区事件池配置")]
    [SerializeField] private RegionEventTable regionEventTable;

    [HideInInspector] public GameEvent currentEvent;

    public event System.Action<GameEvent> OnCurrentEventHappended;

    private void Awake()
    {
        Instance = this;
        currentEvent = null;
        if (regionEventTable == null)
        {
            regionEventTable = Resources.Load<RegionEventTable>("RegionTable/Event/RegionEventTable_Main");
        }
    }

    public IEnumerator RandomEventIE(GameEvent e, Action<string> onEnemyEncounter, bool reopenActionBarIfNoBattle = true)
    {
        if (e == null) yield break;
        SetCurrentEvent(e);
        UIManager uiManager = UIManager.Instance;
        bool hasEnemyEncounter = false;

        if (uiManager != null)
        {
            // Preferred UI API: new code paths should use OpenUIEntry/CloseUIEntry.
            uiManager.CloseUIEntry(UIKey.ActionBar);
            uiManager.OpenUIEntry(UIKey.RandomEvent);
        }
        Debug.Log($"事件：{e.eventDescription}");
        if (e.options != null && e.options.Count > 0)
        {
            if (uiManager != null)
            {
                uiManager.ShowEventNarrationText(e.eventDescription);
            }

            int selectedIndex = -1;
            bool done = false;

            if (uiManager != null)
            {
                uiManager.ShowRandomEventOptions(e.options, (index) =>
                {
                    Debug.Log($"EventManager.RandomEventIE -> 收到选项回调 index={index}");
                    selectedIndex = index;
                    done = true;
                });
            }
            else
            {
                done = true;
            }
            while (!done) yield return null;
            Debug.Log($"EventManager.RandomEventIE -> done=true, selectedIndex={selectedIndex}");

            if (selectedIndex >= 0 && selectedIndex < e.options.Count)
            {
                ExecuteOptionByConfig(
                    e.options[selectedIndex],
                    e.defaultResults,
                    (enemyId) =>
                    {
                        hasEnemyEncounter = true;
                        onEnemyEncounter?.Invoke(enemyId);
                    }
                );
            }
            else
            {
                ExecuteDefaultResultsByConfig(
                    e.defaultResults,
                    (enemyId) =>
                    {
                        hasEnemyEncounter = true;
                        onEnemyEncounter?.Invoke(enemyId);
                    }
                );
            }
        }
        else
        {
            ExecuteDefaultResultsByConfig(
                e.defaultResults,
                (enemyId) =>
                {
                    hasEnemyEncounter = true;
                    onEnemyEncounter?.Invoke(enemyId);
                }
            );
        }

        OnCurrentEventHappended?.Invoke(e);
        Debug.Log("前进随机事件协程结束");
        if (uiManager != null)
        {
            // Preferred UI API: new code paths should use OpenUIEntry/CloseUIEntry.
            uiManager.HideRandomEventOptions();
            uiManager.CloseUIEntry(UIKey.RandomEvent);
            if (reopenActionBarIfNoBattle && !hasEnemyEncounter)
            {
                uiManager.OpenUIEntry(UIKey.ActionBar);
            }
        }
        SetCurrentEvent(null);
    }

    public GameEvent GetRandomEventByRegionCode(string currentRegionCode)
    {
        EventCondition.ValidateRegionCodeOrThrow(currentRegionCode, "CurrentRegionCode");
        if (regionEventTable == null)
        {
            throw new InvalidOperationException("RegionEventTable 未配置，流程中断");
        }

        RegionEventPool pool = GetPoolOrThrow(currentRegionCode);
        return RollFromPoolOrThrow(pool);
    }

    public RegionEventTable GetRegionEventTableForValidation()
    {
        return regionEventTable;
    }

    private RegionEventPool GetPoolOrThrow(string regionCode)
    {
        if (regionEventTable.regionPools == null || regionEventTable.regionPools.Count == 0)
        {
            throw new InvalidOperationException("RegionEventTable 未配置任何地区池，流程中断");
        }

        for (int i = 0; i < regionEventTable.regionPools.Count; i++)
        {
            RegionEventPool pool = regionEventTable.regionPools[i];
            if (pool == null || string.IsNullOrWhiteSpace(pool.regionCode))
            {
                continue;
            }

            EventCondition.ValidateRegionCodeOrThrow(pool.regionCode, "RegionEventTable.regionCode");
            if (string.Equals(pool.regionCode, regionCode, StringComparison.Ordinal))
            {
                if (pool.entries == null || pool.entries.Count == 0)
                {
                    throw new InvalidOperationException($"地区 {regionCode} 未配置事件条目，流程中断");
                }

                return pool;
            }
        }

        throw new InvalidOperationException($"地区 {regionCode} 未配置事件池，流程中断");
    }

    private GameEvent RollFromPoolOrThrow(RegionEventPool pool)
    {
        if (pool == null || pool.entries == null || pool.entries.Count == 0)
        {
            throw new InvalidOperationException("事件池无任何条目，流程中断");
        }

        List<WeightedEventCandidate> candidates = new List<WeightedEventCandidate>();
        for (int i = 0; i < pool.entries.Count; i++)
        {
            RegionEventEntry entry = pool.entries[i];
            if (entry == null || entry.weight <= 0f || entry.eventRef == null)
            {
                continue;
            }

            candidates.Add(new WeightedEventCandidate
            {
                Event = entry.eventRef,
                Weight = entry.weight
            });
        }

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException($"地区 {pool.regionCode} 事件池无有效可抽取条目，流程中断");
        }

        return WeightedPick(candidates);
    }

    private GameEvent WeightedPick(List<WeightedEventCandidate> candidates)
    {
        float total = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            total += Mathf.Max(0f, candidates[i].Weight);
        }

        if (total <= 0f)
        {
            return null;
        }

        float roll = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            WeightedEventCandidate candidate = candidates[i];
            acc += Mathf.Max(0f, candidate.Weight);
            if (roll <= acc)
            {
                return candidate.Event;
            }
        }

        return candidates[candidates.Count - 1].Event;
    }

    private sealed class WeightedEventCandidate
    {
        public GameEvent Event;
        public float Weight;
    }

    public bool HasSpecialItem(int itemId)
    {
        if (InventoryManager.Instance == null)
        {
            return false;
        }

        return InventoryManager.Instance.HasSpecialItem(itemId);
    }

    /// <summary>
    /// 处理事件（异步，等待事件内的所有异步操作）
    /// </summary>
    public IEnumerator ProcessEvent(GameEvent ev)
    {
        if (ev == null) yield break;

        // 显示事件描述（如果UI有显示功能，可以等待玩家点击）
        //UIManager.Instance.ShowEventDescription(ev.description);

        // 如果需要等待玩家点击确认，可以在这里添加 yield return

        // 根据事件是否有异步结果决定是否用协程版本
        //bool hasAsync = ev.results.Exists(r => r.type == EventResultType.Battle || r.type == EventResultType.Dialogue);
        //if (hasAsync)
        //{
        //    yield return StartCoroutine(ev.ApplyResultsAsync());
        //}
        //else
        //{
        //    ev.ApplyResults();
        //}
        //ev.ApplyResults();
    }

    public void SetCurrentEvent(GameEvent e)
    {
        currentEvent = e;
    }

    private string ApplyEventResult(EventResult res)
    {
        if (res == null) return null;

        switch (res.resultType)
        {
            case EventResultType.Stat:
                if (res.statResult != null)
                {
                    StatResultHelper.ApplyResult(res.statResult);
                }
                break;
            case EventResultType.Item:
                if (res.itemResult != null && res.itemResult.count != 0)
                {
                    ApplyItemResult(res.itemResult);
                }
                break;
            case EventResultType.EnemyEncounter:
                return res.enemyId;
        }

        return null;
    }

    private bool ExecuteOptionByConfig(
        EventOption option,
        List<EventResult> fallbackDefaultResults,
        Action<string> onEnemyEncounter
    )
    {
        if (option == null || option.eventOutcomes == null || option.eventOutcomes.Count == 0)
        {
            return ExecuteDefaultResultsByConfig(fallbackDefaultResults, onEnemyEncounter);
        }

        EventOutcome chosenOutcome = ChooseOutcomeByProbability(option.eventOutcomes);
        if (chosenOutcome == null)
        {
            return ExecuteDefaultResultsByConfig(fallbackDefaultResults, onEnemyEncounter);
        }

        if (!string.IsNullOrWhiteSpace(chosenOutcome.outcomeDescription))
        {
            UIManager uiManager = UIManager.Instance;
            if (uiManager != null)
            {
                uiManager.ShowEventNarrationText(chosenOutcome.outcomeDescription);
            }
        }

        if (chosenOutcome.results == null || chosenOutcome.results.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < chosenOutcome.results.Count; i++)
        {
            string enemyId = ApplyEventResult(chosenOutcome.results[i]);
            if (!string.IsNullOrWhiteSpace(enemyId))
            {
                onEnemyEncounter?.Invoke(enemyId);
                return true;
            }
        }

        return false;
    }

    private bool ExecuteDefaultResultsByConfig(
        List<EventResult> defaultResults,
        Action<string> onEnemyEncounter
    )
    {
        if (defaultResults == null || defaultResults.Count == 0) return false;

        for (int i = 0; i < defaultResults.Count; i++)
        {
            string enemyId = ApplyEventResult(defaultResults[i]);
            if (!string.IsNullOrWhiteSpace(enemyId))
            {
                onEnemyEncounter?.Invoke(enemyId);
                return true;
            }
        }

        return false;
    }

    private EventOutcome ChooseOutcomeByProbability(List<EventOutcome> outcomes)
    {
        if (outcomes == null || outcomes.Count == 0) return null;

        float total = 0f;
        List<EventOutcome> valid = new List<EventOutcome>();
        for (int i = 0; i < outcomes.Count; i++)
        {
            EventOutcome outcome = outcomes[i];
            if (outcome == null) continue;

            float p = Mathf.Max(0f, outcome.probability);
            if (p > 0f)
            {
                total += p;
                valid.Add(outcome);
            }
        }

        if (valid.Count == 0)
        {
            // 全部概率<=0 时，回退到第一个非空结果组
            for (int i = 0; i < outcomes.Count; i++)
            {
                if (outcomes[i] != null) return outcomes[i];
            }
            return null;
        }

        float rand = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        for (int i = 0; i < valid.Count; i++)
        {
            EventOutcome outcome = valid[i];
            acc += Mathf.Max(0f, outcome.probability);
            if (rand <= acc) return outcome;
        }

        return valid[valid.Count - 1];
    }

    private void ApplyItemResult(ItemResult itemResult)
    {
        if (itemResult == null || itemResult.itemID == 0 || itemResult.count == 0) return;
        if (InventoryManager.Instance == null) return;

        if (itemResult.count > 0)
        {
            InventoryManager.Instance.AddItem(itemResult.itemID, itemResult.count);
        }
        else
        {
            int removeCount = Mathf.Abs(itemResult.count);
            InventoryManager.Instance.RemoveItem(itemResult.itemID, removeCount);
        }
    }
}

