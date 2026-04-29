using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopCatalog", menuName = "GoHome/Shop/Catalog")]
public class ShopCatalogDefinition : ScriptableObject
{
    [SerializeField] private List<ShopCommodityDefinition> commodities = new List<ShopCommodityDefinition>();

    public IReadOnlyList<ShopCommodityDefinition> Commodities => commodities;
}
