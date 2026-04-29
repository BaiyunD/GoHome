using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestPage : MonoBehaviour
{
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text traitLinesText;
    [SerializeField] private Button confirmButton;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }
    }

    public void Show(RestSettlement settlement)
    {
        Refresh(settlement);
        gameObject.SetActive(true);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Refresh(RestSettlement settlement)
    {
        if (settlement == null)
        {
            return;
        }

        if (statsText == null)
        {
            return;
        }

        statsText.text =
            $"第 {settlement.DayAfter} 天\n" +
            BuildStatLine("体力", settlement.DisplayedEnergyDelta, settlement.EnergyAfter, settlement.EnergyMaxAfter) + "\n" +
            BuildStatLine("饥饿", settlement.DisplayedHungerDelta, settlement.HungerAfter, settlement.HungerMaxAfter) + "\n" +
            BuildStatLine("生命", settlement.DisplayedHPDelta, settlement.HPAfter, settlement.HPMaxAfter);
        if (traitLinesText != null)
        {
            traitLinesText.text = RestLogRenderService.BuildItemThenTraitText(settlement);
        }
    }

    private void OnConfirmClicked()
    {
        if (RestManager.Instance != null)
        {
            RestManager.Instance.ConfirmRest();
        }
    }

    private static string BuildStatLine(string label, int delta, int current, int max)
    {
        return string.Format("{0} {1}{2} ({3}/{4})", label, delta >= 0 ? "+" : "-", Mathf.Abs(delta), current, max);
    }

}

