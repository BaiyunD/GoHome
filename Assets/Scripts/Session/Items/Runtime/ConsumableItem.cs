using UnityEngine;

public abstract class ConsumableItem : ItemBase
{
    public void Use(int level)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        OnUse(GameManager.Instance, level);
    }

    protected abstract void OnUse(GameManager gameManager, int level);
}

