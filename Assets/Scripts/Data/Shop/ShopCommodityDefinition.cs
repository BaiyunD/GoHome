using UnityEngine;

public enum ShopTradePermission
{
    BuyAndSell = 0,
    BuyOnly = 1,
    SellOnly = 2
}

[CreateAssetMenu(fileName = "ShopCommodity", menuName = "GoHome/Shop/Commodity")]
public class ShopCommodityDefinition : ScriptableObject
{
    [SerializeField] private int commodityId = 1;
    [SerializeField] private int itemId = 1;
    [SerializeField] private int tradeCount = 1;
    [SerializeField] private float buyPrice = 0.1f;
    [SerializeField] private float sellPrice = 0.1f;
    [SerializeField] private ShopTradePermission tradePermission = ShopTradePermission.BuyAndSell;
    [SerializeField] private bool isSellAll = true;
    [SerializeField] private bool isPriceIncreaseOnBuy = true;

    public int CommodityId => commodityId;
    public int ItemId => itemId;
    public int TradeCount => tradeCount;
    public float BuyPrice => buyPrice;
    public float SellPrice => sellPrice;
    public ShopTradePermission TradePermission => tradePermission;
    public bool IsSellAll => isSellAll;
    public bool IsPriceIncreaseOnBuy => isPriceIncreaseOnBuy;
}
