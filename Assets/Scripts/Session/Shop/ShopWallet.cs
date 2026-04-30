using System;

[Serializable]
public struct ShopWalletSnapshot
{
    public int Points;
    public ShopBuyPriceSnapshot[] BuyPriceSnapshots;
}

[Serializable]
public struct ShopBuyPriceSnapshot
{
    public int CommodityId;
    public float CurrentBuyPrice;
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

    public bool TrySpendPoints(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Points < amount)
        {
            return false;
        }

        Points -= amount;
        return true;
    }

    public void RefundPoints(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Points += amount;
    }
}
