using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndPage : MonoBehaviour
{
    [SerializeField] private TMP_Text endText;

    private void Start()
    {
        GameManager.Instance.GameOverEvent += OnGameOver;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOverEvent -= OnGameOver;
        }
    }

    private void OnGameOver()
    {
        UpdateEndText(EndingManager.Instance.message);
    }

    private void UpdateEndText(string message)
    {
        endText.text = message;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

