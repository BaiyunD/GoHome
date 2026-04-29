using System;

[Serializable]
public struct ShopWalletSnapshot
{
    public int Points;
}

public sealed class ShopWallet
{
    public int Points
    {
        get; private set;
    }

    public void InitializeForNewGame()
    {
        Points = 0;
    }

    public void ApplySnapshot(ShopWalletSnapshot snapshot)
    {
        Points = snapshot.Points < 0 ? 0 : snapshot.Points;
    }

    public ShopWalletSnapshot ExportSnapshot()
    {
        return new ShopWalletSnapshot
        {
            Points = Points
        };
    }

    public void AddPoints(int delta)
    {
        if (delta <= 0)
        {
            return;
        }

        Points += delta;
    }
}
