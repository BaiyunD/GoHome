using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureAppRoot()
    {
        if (AppRoot.Instance != null)
        {
            return;
        }

        AppRoot existing = Object.FindFirstObjectByType<AppRoot>();
        if (existing != null)
        {
            return;
        }

        GameObject go = new GameObject("AppRoot");
        go.AddComponent<AppRoot>();
    }
}

