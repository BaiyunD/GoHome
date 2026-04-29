using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionManager : MonoBehaviour
{
    public static ActionManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("ActionManager.Awake -> 检测到重复 ActionManager，请确保场景中只挂载一个。");
            return;
        }

        Instance = this;
    }

    public void TryAdvance()
    {
        if (AdvanceFlowController.Instance == null)
        {
            Debug.LogError("ActionManager.TryAdvance -> AdvanceFlowController 未挂载，无法执行前进流程。");
            return;
        }

        AdvanceFlowController.Instance.TryAdvance();
    }

    public void TryExplore()
    {
        if (AdvanceFlowController.Instance == null)
        {
            Debug.LogError("ActionManager.TryExplore -> AdvanceFlowController 未挂载，无法执行探索流程。");
            return;
        }

        AdvanceFlowController.Instance.TryExplore();
    }

    public void TryRest()
    {
        if (RestManager.Instance == null)
        {
            Debug.LogError("ActionManager.TryRest -> RestManager 未挂载，无法执行休息流程。");
            return;
        }

        RestBeginResult result = RestManager.Instance.TryBeginRest();
        if (result == RestBeginResult.NeedSecondClick)
        {
            UIManager.Instance.ShowEventNarrationText("当前还有富余体力哦~还是想要休息的话再来吧");
        }
    }

    public static int GetCurrentRegionId()
    {
        if (RouteProgressManager.Instance == null) return 0;
        return RouteProgressManager.Instance.GetCurrentMainRegionId();
    }

    public static RegionActionContext BuildCurrentActionContext()
    {
        int mainRegionId = RouteProgressManager.Instance != null
            ? RouteProgressManager.Instance.GetCurrentMainRegionId()
            : -1;
        int subRegionId = RouteProgressManager.Instance != null
            ? RouteProgressManager.Instance.GetCurrentSubRegionId()
            : -1;
        int energy = 0;
        if (PlayerResourceService.Instance != null &&
            PlayerResourceService.Instance.TryGetValue(PlayerResourceType.Energy, out float energyValue))
        {
            energy = Mathf.RoundToInt(energyValue);
        }

        int hunger = 0;
        if (PlayerResourceService.Instance != null &&
            PlayerResourceService.Instance.TryGetValue(PlayerResourceType.Hunger, out float hungerValue))
        {
            hunger = Mathf.RoundToInt(hungerValue);
        }

        return new RegionActionContext(mainRegionId, subRegionId, energy, hunger);
    }
}

