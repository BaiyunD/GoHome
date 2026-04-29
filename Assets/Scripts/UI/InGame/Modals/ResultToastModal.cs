using System.Collections;
using TMPro;
using UnityEngine;

public class ResultToastModal : MonoBehaviour
{
    public static ResultToastModal Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text messageText;

    [Header("Behavior")]
    /// <summary>
    /// 提示保持显示的秒数。
    /// </summary>
    [SerializeField] private float visibleSeconds = 3f;

    private Coroutine hideCoroutine;
    private bool isShowRequestInProgress;

    /// <summary>
    /// 初始化时检查文本引用，并默认隐藏提示框。
    /// </summary>
    protected virtual void Awake()
    {
        Instance = this;
        if (messageText == null)
        {
            Debug.LogError("ResultToastModal: messageText 未绑定，请在 Inspector 手动绑定。", this);
        }
        if (!isShowRequestInProgress)
        {
            HideImmediate();
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 显示提示文本，并在指定秒数后自动隐藏。若在隐藏前再次调用，会重置计时并更新文本。
    /// </summary>
    public void Show(string message)
    {
        string safeMessage = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
        if (messageText != null)
        {
            messageText.text = safeMessage;
        }

        isShowRequestInProgress = true;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterSecondsCoroutine());
        isShowRequestInProgress = false;
    }

    /// <summary>
    /// 通过全局单例显示提示；若未配置提示框则输出警告。
    /// </summary>
    public static void ShowShared(string message)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResultToast(message);
            return;
        }

        if (Instance == null)
        {
            Debug.LogWarning("ResultToastModal: 场景中未找到可用实例，无法显示提示。");
            return;
        }

        Instance.Show(message);
    }

    /// <summary>
    /// 立刻隐藏提示，并停止正在进行的自动隐藏计时。
    /// </summary>
    public void HideImmediate()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 延时隐藏协程（每次 Show 都会重置）。
    /// </summary>
    private IEnumerator HideAfterSecondsCoroutine()
    {
        float seconds = visibleSeconds <= 0f ? 3f : visibleSeconds;
        yield return new WaitForSeconds(seconds);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideResultToast();
        }
        else
        {
            gameObject.SetActive(false);
        }
        hideCoroutine = null;
    }
}

