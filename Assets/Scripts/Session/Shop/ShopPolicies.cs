using UnityEngine;

public interface IShopPointsPolicy
{
    int CalculatePoints(float pricePerTrade);
}

public sealed class DecimalPricePointsPolicy : IShopPointsPolicy
{
    public int CalculatePoints(float pricePerTrade)
    {
        return Mathf.RoundToInt(pricePerTrade * 10f);
    }
}

public interface IShopTransactionPolicy
{
    bool Validate(ShopTradeRequest request, out ShopTradeReasonCode reasonCode);
}

public sealed class DefaultShopTransactionPolicy : IShopTransactionPolicy
{
    public bool Validate(ShopTradeRequest request, out ShopTradeReasonCode reasonCode)
    {
        if (request.CommodityId <= 0 || request.Times <= 0)
        {
            reasonCode = ShopTradeReasonCode.InvalidRequest;
            return false;
        }

        reasonCode = ShopTradeReasonCode.Success;
        return true;
    }
}
