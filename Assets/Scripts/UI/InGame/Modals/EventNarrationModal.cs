using TMPro;
using UnityEngine;

public class EventNarrationModal : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void SetText(string message)
    {
        if (messageText == null)
        {
            Debug.LogWarning("EventNarrationModal.SetText -> messageText 未绑定", this);
            return;
        }

        messageText.text = message;
    }

    public void ClearText()
    {
        if (messageText == null)
        {
            Debug.LogWarning("EventNarrationModal.ClearText -> messageText 未绑定", this);
            return;
        }

        messageText.text = string.Empty;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        gameObject.SetActive(false);
    }
}

