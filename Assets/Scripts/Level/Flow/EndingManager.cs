using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EndingType
{
    None,
    Win,   // 到家
    Lose   // 死在街头
}

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance {  get; private set; }

    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endText;

    //private EndingType endingType = EndingType.None;
    private bool hasEnded = false;

    public string message;

    private void Awake()
    {
        Instance = this;
        endPanel.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.GameOverEvent += TriggerEnding;
    }

    private void OnDestroy()
    {
        GameManager.Instance.GameOverEvent -= TriggerEnding;
    }

    public void TriggerEnding()
    {
        if (hasEnded) return;
        hasEnded = true;

        int distance = RouteProgressManager.Instance != null ? RouteProgressManager.Instance.GetDistance() : 0;
        if (distance >= GameManager.Instance.HomeDistance)
        {
            message = "你终于回到了家乡，虽然衣衫褴褛，但你还活着。";
        }
        else if (PlayerStateManager.Instance != null &&
            PlayerStateManager.Instance.Current != null &&
            PlayerStateManager.Instance.CurrentHp <= 0f)
        {
            message = "在某个寒冷的夜晚，你没能撑过去……";
        }
        else
        { 
            Debug.LogWarning("错误！未满足结局条件！");
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenEndPanelFromFlow();
        }
        else
        {
            Debug.LogWarning("EndingManager.TriggerEnding -> UIManager 未挂载，无法打开结局面板");
        }
    }

    public void Reset()
    {
        hasEnded = false;
        endPanel.SetActive(false);
    }
}

