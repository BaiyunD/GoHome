using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomEventPanel : MonoBehaviour
{
    [SerializeField] public TMP_Text optionText1;
    [SerializeField] public TMP_Text optionText2;
    [SerializeField] public Button optionButton1;
    [SerializeField] public Button optionButton2;

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void ShowOptions(List<EventOption> options, Action<int> onSelected)
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("RandomEventPanel.ShowOptions -> options 为空，隐藏选项面板", this);
            HideOptions();
            return;
        }

        Debug.Log($"RandomEventPanel.ShowOptions -> options.Count={options.Count}", this);
        Show();
        BindOptionButton(optionButton1, optionText1, options, 0, onSelected);
        BindOptionButton(optionButton2, optionText2, options, 1, onSelected);
    }

    public void HideOptions()
    {
        optionButton1.onClick.RemoveAllListeners();
        optionButton2.onClick.RemoveAllListeners();

        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        Hide();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void BindOptionButton(
        Button button,
        TMP_Text text,
        List<EventOption> options,
        int index,
        Action<int> onSelected
    )
    {
        if (button == null)
        {
            Debug.LogWarning($"RandomEventPanel.BindOptionButton -> button{index + 1} 未绑定", this);
            return;
        }

        if (text == null)
        {
            Debug.LogWarning($"RandomEventPanel.BindOptionButton -> text{index + 1} 未绑定，但继续绑定按钮监听", this);
        }

        button.onClick.RemoveAllListeners();
        bool available = options.Count > index;

        button.gameObject.SetActive(available);
        button.interactable = available;
        if (text != null)
        {
            text.text = available ? options[index].optionText : string.Empty;
        }

        if (available)
        {
            button.onClick.AddListener(() =>
            {
                Debug.Log($"RandomEventPanel -> 点击选项按钮 index={index}", this);
                onSelected?.Invoke(index);
            });
        }
    }
}

