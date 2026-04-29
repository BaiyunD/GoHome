using System;
using UnityEngine;

public sealed class EnemyStateManager : MonoBehaviour
{
    public static EnemyStateManager Instance
    {
        get; private set;
    }

    public EnemyRuntime Current
    {
        get; private set;
    }

    public float CurrentHp
    {
        get
        {
            return Current != null ? Current.CurrentHp : 0f;
        }
        set
        {
            if (Current == null)
            {
                return;
            }

            Current.SetCurrentHp(value);
        }
    }

    public event Action<EnemyRuntime> EnemyRuntimeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void BeginBattle(EnemyData enemyTemplate)
    {
        EnemyData runtimeData = CreateRuntimeTemplate(enemyTemplate);
        if (Current == null)
        {
            Current = new EnemyRuntime(runtimeData);
        }
        else
        {
            Current.ResetFromTemplate(runtimeData);
        }

        EnemyRuntimeChanged?.Invoke(Current);
    }

    public void ClearCurrent()
    {
        if (Current != null && Current.RuntimeData != null)
        {
            Destroy(Current.RuntimeData);
        }

        Current = null;
        EnemyRuntimeChanged?.Invoke(null);
    }

    private static EnemyData CreateRuntimeTemplate(EnemyData enemyTemplate)
    {
        return enemyTemplate != null
            ? Instantiate(enemyTemplate)
            : ScriptableObject.CreateInstance<EnemyData>();
    }
}
