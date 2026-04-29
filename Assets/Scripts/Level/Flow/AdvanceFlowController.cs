using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum AdvanceFlowState
{
    Idle = 0,
    Running = 1,
    Transit = 2
}

public enum AdvanceFlowStage
{
    Prepare = 0,
    AdvanceDistance = 1,
    SelectFeedback = 2,
    Execute = 3,
    Finalize = 4
}

public enum AdvancePrimaryFeedback
{
    Supply = 0,
    RandomEvent = 1,
    Enemy = 2
}

public class AdvanceFlowController : MonoBehaviour
{
    private sealed class AdvanceFlowPipelineContext
    {
        public bool IsAdvance;
        public string CurrentRegionCode;
        public AdvancePrimaryFeedback PrimaryFeedback;
    }

    private sealed class BranchExecutionResult
    {
        public bool ShouldStartBattle;
        public string EnemyId;
    }

    public static AdvanceFlowController Instance { get; private set; }

    [Header("概率分流（总和建议100）")]
    [SerializeField] private float supplyProbability = 60f;
    [SerializeField] private float randomEventProbability = 20f;
    [SerializeField] private float enemyProbability = 20f;
    [Header("物资掉落配置")]
    [SerializeField] private RegionLootTable regionLootTable;

    public AdvanceFlowState State { get; private set; } = AdvanceFlowState.Idle;
    public RegionLootTable RegionLootTable => regionLootTable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("AdvanceFlowController.Awake -> 检测到重复 AdvanceFlowController，请确保场景中只挂载一个。");
            return;
        }

        Instance = this;
        if (regionLootTable == null)
        {
            regionLootTable = Resources.Load<RegionLootTable>("RegionTable/Loot/RegionLootTable_Main");
        }
    }

    public void TryAdvance()
    {
        TryRunFlow(isAdvance: true);
    }

    public void TryExplore()
    {
        TryRunFlow(isAdvance: false);
    }

    private void TryRunFlow(bool isAdvance)
    {
        if (State == AdvanceFlowState.Running || State == AdvanceFlowState.Transit)
        {
            return;
        }

        if (PlayerResourceService.Instance == null)
        {
            return;
        }

        if (!PlayerResourceService.Instance.TryGetValue(PlayerResourceType.Energy, out float currentEnergy))
        {
            UIManager.Instance.ShowEventNarrationText("精力数据暂不可用，无法行动");
            return;
        }

        if (currentEnergy <= 0f)
        {
            UIManager.Instance.ShowEventNarrationText("你已经很劳累了，先休息一下再来吧");
            return;
        }

        StartCoroutine(RunFlowCoroutine(isAdvance));
    }

    private IEnumerator RunFlowCoroutine(bool isAdvance)
    {
        State = AdvanceFlowState.Running;
        int seed = ResolveRunSeed();
        UnityEngine.Random.InitState(seed);

        AdvanceFlowPipelineContext context = new AdvanceFlowPipelineContext
        {
            IsAdvance = isAdvance
        };

        Exception flowException = null;
        if (!TryExecuteSyncStage(() => ExecutePrepareStage(context), out flowException))
        {
            HandleFlowFailure(flowException);
            throw flowException;
        }

        yield return RunStageCoroutineSafely(
            ExecuteAdvanceDistanceStageCoroutine(context),
            (ex) => flowException = ex
        );
        if (flowException != null)
        {
            HandleFlowFailure(flowException);
            throw flowException;
        }

        if (!TryExecuteSyncStage(() => ExecuteSelectFeedbackStage(context), out flowException))
        {
            HandleFlowFailure(flowException);
            throw flowException;
        }

        yield return RunStageCoroutineSafely(
            ExecuteStageCoroutine(context),
            (ex) => flowException = ex
        );
        if (flowException != null)
        {
            HandleFlowFailure(flowException);
            throw flowException;
        }

        ExecuteFinalizeStage("流程完成");
    }

    private bool TryExecuteSyncStage(Action stageAction, out Exception stageException)
    {
        stageException = null;
        try
        {
            stageAction?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            stageException = ex;
            return false;
        }
    }

    private IEnumerator RunStageCoroutineSafely(IEnumerator stageCoroutine, Action<Exception> onException)
    {
        if (stageCoroutine == null)
        {
            yield break;
        }

        while (true)
        {
            object current;
            try
            {
                if (!stageCoroutine.MoveNext())
                {
                    yield break;
                }

                current = stageCoroutine.Current;
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                yield break;
            }

            yield return current;
        }
    }

    private void HandleFlowFailure(Exception ex)
    {
        if (ex == null)
        {
            return;
        }

        ExecuteFinalizeStage($"流程异常终止：{ex.Message}", false);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEventNarrationText(ex.Message);
        }
    }

    private void ExecutePrepareStage(AdvanceFlowPipelineContext context)
    {
        if (context == null)
        {
            throw new InvalidOperationException("AdvanceFlowController.Prepare -> context 不能为空");
        }

        if (PlayerResourceService.Instance == null)
        {
            throw new InvalidOperationException("AdvanceFlowController.Prepare -> PlayerResourceService 未挂载");
        }

        PlayerResourceService.Instance.TryConsume(PlayerResourceType.Energy, 10, "AdvanceFlowController.Prepare");

        float currentHunger = 0f;
        bool hasHunger = PlayerResourceService.Instance.TryGetValue(
            PlayerResourceType.Hunger,
            out currentHunger
        );
        if (!hasHunger || currentHunger > 0f)
        {
            PlayerResourceService.Instance.TryConsume(
                PlayerResourceType.Hunger,
                5,
                "AdvanceFlowController.Prepare"
            );
        }
        else if (UnityEngine.Random.value < 0.3f)
        {
            PlayerResourceService.Instance.TryConsume(
                PlayerResourceType.Health,
                1,
                "AdvanceFlowController.Prepare"
            );
        }

        PublishStageResult(AdvanceFlowStage.Prepare, true, "Prepare 阶段资源结算完成，进入前进流程");
    }

    private IEnumerator ExecuteAdvanceDistanceStageCoroutine(AdvanceFlowPipelineContext context)
    {
        if (context.IsAdvance)
        {
            if (RouteProgressManager.Instance == null)
            {
                throw new InvalidOperationException("AdvanceFlowController.AdvanceDistance -> RouteProgressManager 未挂载");
            }

            RouteProgressManager.Instance.Advance(1);
            PublishStageResult(AdvanceFlowStage.AdvanceDistance, true, "前进距离已提交 +1");
            Debug.Log("AdvanceFlowController.AdvanceDistance -> 已先提交 distance +1，再进入反馈选择");
            yield break;
        }

        PublishStageResult(AdvanceFlowStage.AdvanceDistance, true, "当前为探索流程，跳过距离推进");
        yield break;
    }

    private void ExecuteSelectFeedbackStage(AdvanceFlowPipelineContext context)
    {
        int subRegionId = RouteProgressManager.Instance != null
            ? RouteProgressManager.Instance.GetCurrentSubRegionId()
            : -1;
        if (subRegionId < 0)
        {
            throw new InvalidOperationException("当前子地区编号异常，流程已中断");
        }

        context.CurrentRegionCode = BuildCurrentRegionCode();

        float routeType = UnityEngine.Random.Range(0f, GetTotalProbability());
        if (routeType < supplyProbability)
        {
            context.PrimaryFeedback = AdvancePrimaryFeedback.Supply;
        }
        else if (routeType < supplyProbability + randomEventProbability)
        {
            context.PrimaryFeedback = AdvancePrimaryFeedback.RandomEvent;
        }
        else
        {
            context.PrimaryFeedback = AdvancePrimaryFeedback.Enemy;
        }

        PublishStageResult(
            AdvanceFlowStage.SelectFeedback,
            true,
            $"主反馈={context.PrimaryFeedback}, regionCode={context.CurrentRegionCode}"
        );
    }

    private IEnumerator ExecuteStageCoroutine(AdvanceFlowPipelineContext context)
    {
        PublishStageResult(
            AdvanceFlowStage.Execute,
            true,
            $"执行主反馈={context.PrimaryFeedback}"
        );

        BranchExecutionResult branchResult = null;
        switch (context.PrimaryFeedback)
        {
            case AdvancePrimaryFeedback.Supply:
                yield return RunSupplyBranchCoroutine(context.CurrentRegionCode, (result) => branchResult = result);
                break;
            case AdvancePrimaryFeedback.RandomEvent:
                yield return RunRandomEventBranchCoroutine(context.CurrentRegionCode, (result) => branchResult = result);
                break;
            case AdvancePrimaryFeedback.Enemy:
                yield return RunEnemyBranchCoroutine(context.CurrentRegionCode, (result) => branchResult = result);
                break;
            default:
                throw new InvalidOperationException(
                    $"未知主反馈类型：{context.PrimaryFeedback}"
                );
        }

        if (branchResult != null && branchResult.ShouldStartBattle && !string.IsNullOrWhiteSpace(branchResult.EnemyId))
        {
            yield return RunBattleByEnemyIdCoroutine(context.CurrentRegionCode, branchResult.EnemyId);
        }

        yield break;
    }

    private void ExecuteFinalizeStage(string detail, bool success = true)
    {
        State = AdvanceFlowState.Idle;
        PublishStageResult(AdvanceFlowStage.Finalize, success, detail);
    }

    private IEnumerator RunSupplyBranchCoroutine(
        string currentRegionCode,
        Action<BranchExecutionResult> onCompleted
    )
    {
        RegionLootService.LootRollResult mainResult = RegionLootService.RollAndGrant(currentRegionCode, regionLootTable);
        AdvanceSupplyEffectContext effectContext = new AdvanceSupplyEffectContext
        {
            RegionCode = currentRegionCode,
            RegionLootTable = regionLootTable,
            MainResult = mainResult
        };

        List<AdvanceSupplyEffectDispatchResult> extraResults =
            ItemEffectDispatcher.ApplyAdvanceSupplyForInventory(effectContext);
        for (int i = 0; i < extraResults.Count; i++)
        {
            AdvanceSupplyEffectDispatchResult extraResult = extraResults[i];
            PublishStageResult(
                AdvanceFlowStage.Execute,
                true,
                $"额外物资触发 item={extraResult.ItemId}, source={extraResult.Source}, empty={extraResult.LootResult.IsEmpty}"
            );
        }

        UIManager.Instance.ShowEventNarrationText(
            BuildSupplyNarration(mainResult, extraResults)
        );
        onCompleted?.Invoke(new BranchExecutionResult
        {
            ShouldStartBattle = false
        });
        yield break;
    }

    private static string BuildSupplyNarration(
        RegionLootService.LootRollResult mainResult,
        List<AdvanceSupplyEffectDispatchResult> extraResults)
    {
        StringBuilder sb = new StringBuilder(RegionLootService.BuildNarration(mainResult));
        if (extraResults == null || extraResults.Count == 0)
        {
            return sb.ToString();
        }

        for (int i = 0; i < extraResults.Count; i++)
        {
            AdvanceSupplyEffectDispatchResult extraResult = extraResults[i];
            string itemDisplayName = !string.IsNullOrWhiteSpace(extraResult.ItemDisplayName)
                ? extraResult.ItemDisplayName
                : $"Item({extraResult.ItemId})";
            sb.Append("【")
                .Append(itemDisplayName)
                .Append("：")
                .Append(RegionLootService.BuildNarration(extraResult.LootResult))
                .Append("】");
        }

        return sb.ToString();
    }

    private IEnumerator RunRandomEventBranchCoroutine(
        string currentRegionCode,
        Action<BranchExecutionResult> onCompleted
    )
    {
        GameEvent randomEvent = RollRandomEventByRegion(currentRegionCode);
        if (randomEvent == null)
        {
            throw new InvalidOperationException("当前地区没有可触发的随机事件");
        }

        string enemyIdFromEvent = null;
        yield return StartCoroutine(
            EventManager.Instance.RandomEventIE(
                randomEvent,
                (enemyId) => enemyIdFromEvent = enemyId
            )
        );

        onCompleted?.Invoke(new BranchExecutionResult
        {
            ShouldStartBattle = !string.IsNullOrWhiteSpace(enemyIdFromEvent),
            EnemyId = enemyIdFromEvent
        });
    }

    private IEnumerator RunEnemyBranchCoroutine(
        string currentRegionCode,
        Action<BranchExecutionResult> onCompleted
    )
    {
        EnemyData enemy = RollRandomEnemyByRegion(currentRegionCode);

        if (enemy == null)
        {
            throw new InvalidOperationException("该地区无敌人配置");
        }

        onCompleted?.Invoke(new BranchExecutionResult
        {
            ShouldStartBattle = true,
            EnemyId = enemy.EnemyId
        });
        yield break;
    }

    private IEnumerator RunBattleByEnemyIdCoroutine(string currentRegionCode, string enemyId)
    {
        if (BattleManager.Instance == null)
        {
            throw new InvalidOperationException("BattleManager 未挂载，流程中断");
        }

        State = AdvanceFlowState.Transit;
        bool finished = false;
        bool started = BattleManager.Instance.StartBattleByEnemyId(
            currentRegionCode,
            enemyId,
            (_) => finished = true
        );

        if (!started)
        {
            throw new InvalidOperationException("战斗启动失败，流程中断");
        }

        while (!finished)
        {
            yield return null;
        }
    }

    private GameEvent RollRandomEventByRegion(string currentRegionCode)
    {
        if (EventManager.Instance == null)
        {
            throw new InvalidOperationException("EventManager 未挂载，流程中断");
        }

        return EventManager.Instance.GetRandomEventByRegionCode(currentRegionCode);
    }

    private EnemyData RollRandomEnemyByRegion(string currentRegionCode)
    {
        if (EnemyPoolService.Instance == null)
        {
            throw new InvalidOperationException("EnemyPoolService 未挂载，流程中断");
        }

        return EnemyPoolService.Instance.GetRandomEnemyByRegionCode(currentRegionCode);
    }

    private string BuildCurrentRegionCode()
    {
        if (RouteProgressManager.Instance == null)
        {
            throw new InvalidOperationException("RouteProgressManager 未挂载，流程中断");
        }

        int mainRegionId = RouteProgressManager.Instance.GetCurrentMainRegionId();
        int subRegionId = RouteProgressManager.Instance.GetCurrentSubRegionId();
        if (mainRegionId < 0 || subRegionId < 0)
        {
            throw new InvalidOperationException("当前地区编号异常，流程已中断");
        }

        string code = $"{mainRegionId}_{subRegionId}";
        EventCondition.ValidateRegionCodeOrThrow(code, "CurrentRegionCode");
        return code;
    }

    private float GetTotalProbability()
    {
        float total = Mathf.Max(0f, supplyProbability) +
            Mathf.Max(0f, randomEventProbability) +
            Mathf.Max(0f, enemyProbability);
        return total <= 0f ? 100f : total;
    }

    private void PublishStageResult(AdvanceFlowStage stage, bool success, string detail)
    {
        if (success)
        {
            Debug.Log($"AdvanceFlowController.Stage[{stage}] -> {detail}");
        }
        else
        {
            Debug.LogError($"AdvanceFlowController.Stage[{stage}] -> {detail}");
        }
    }

    private int ResolveRunSeed()
    {
        return Environment.TickCount;
    }
}

