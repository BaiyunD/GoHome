using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class EventSystemBootstrap : MonoBehaviour
{
    private static EventSystemBootstrap _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        EnsureSingleton();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureEventSystemComponents(gameObject);
        DisableDuplicateEventSystems();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _instance = null;
        }
    }

    private static void EnsureSingleton()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("EventSystemBootstrap");
        bootstrap.AddComponent<EventSystemBootstrap>();
    }

    private static void EnsureEventSystemComponents(GameObject target)
    {
        if (target.GetComponent<EventSystem>() == null)
        {
            target.AddComponent<EventSystem>();
        }

        if (target.GetComponent<StandaloneInputModule>() == null)
        {
            target.AddComponent<StandaloneInputModule>();
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DisableDuplicateEventSystems();
    }

    private static void DisableDuplicateEventSystems()
    {
        EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>(true);
        EventSystem keep = _instance != null ? _instance.GetComponent<EventSystem>() : null;

        if (keep == null && eventSystems.Length > 0)
        {
            keep = eventSystems[0];
        }

        for (int i = 0; i < eventSystems.Length; i++)
        {
            EventSystem current = eventSystems[i];
            if (current == null)
            {
                continue;
            }

            bool shouldEnable = current == keep && current.gameObject.activeInHierarchy;
            if (current.enabled != shouldEnable)
            {
                current.enabled = shouldEnable;
            }
        }
    }
}
