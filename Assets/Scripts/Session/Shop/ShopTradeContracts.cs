using System;
using UnityEngine;

public enum ShopTradeOperation
{
    Buy = 0,
    Sell = 1
}

[Serializable]
public struct ShopTradeRequest
{
    public int CommodityId;
    public ShopTradeOperation Operation;
    public int Times;
}

[Serializable]
public struct ShopItemDelta
{
    public int ItemId;
    public int DeltaCount;
}

public enum ShopTradeReasonCode
{
    Success = 0,
    InvalidRequest = 1,
    InsufficientCash = 2,
    InsufficientInventory = 3,
    CommodityMissing = 4,
    FeatureDisabled = 5
}

[Serializable]
public struct ShopTradeResult
{
    public bool Success;
    public ShopTradeReasonCode ReasonCode;
    public float DeltaCash;
    public int DeltaPoints;
    public ShopItemDelta DeltaItems;
    public string ItemName;
    public int TradeCount;
    public float TotalPrice;
    public ShopTradeOperation Operation;
}

[Serializable]
public struct ShopCommodityViewModel
{
    public int CommodityId;
    public int ItemId;
    public string ItemName;
    public string ItemDescription;
    public Sprite Icon;
    public int OwnedCount;
    public int TradeCount;
    public float BuyPrice;
    public float SellPrice;
    public bool CanBuy;
    public bool CanSell;
    public bool IsSellAll;
}
