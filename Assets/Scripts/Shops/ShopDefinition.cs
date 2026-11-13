using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Shops/Shop Definition")]
public sealed class ShopDefinition : ScriptableObject
{
    public string shopName = "General Store";
    public string category = "General";
    public ShopStockRule[] stock = Array.Empty<ShopStockRule>();
    public Item[] buyItems = Array.Empty<Item>();
    public ItemType[] buyCategories = Array.Empty<ItemType>();
    public Item[] sellItems = Array.Empty<Item>();
    public ItemType[] sellCategories = Array.Empty<ItemType>();
    [Min(0)] public float purchaseMultiplier = 1.25f;
    [Min(0)] public float saleMultiplier = .5f;
    [Min(0)] public float restockInterval = 60;
    [Min(1)] public int purchasedItemCapacity = 10;
    public bool Accepts(Item item) => item != null && (!item.tradeRestricted || Array.IndexOf(buyItems, item) >= 0) &&
        (Array.IndexOf(buyItems, item) >= 0 || Array.IndexOf(buyCategories, item.itemType) >= 0);
    public bool Sells(Item item) => item != null && (Array.Exists(stock, r => r != null && r.item == item) ||
        Array.IndexOf(sellItems, item) >= 0 || Array.IndexOf(sellCategories, item.itemType) >= 0);
    public ShopStockRule Rule(Item item) => Array.Find(stock, r => r != null && r.item == item);
    public int Price(Item item, bool buying)
    {
        if (item == null) return 0;
        var rule = Rule(item);
        int value = rule == null ? -1 : buying ? rule.purchasePrice : rule.salePrice;
        double raw = value >= 0 ? value : Math.Max(0, item.price) * (double)Mathf.Max(0, buying ? purchaseMultiplier : saleMultiplier);
        return (int)Math.Min(int.MaxValue, Math.Max(0, Math.Ceiling(raw)));
    }
}
[Serializable]
public sealed class ShopStockRule
{
    public Item item;
    [Min(0)] public int initial = 3;
    [Min(1)] public int maximum = 10;
    [Min(0)] public int restockAmount = 1;
    public int purchasePrice = -1;
    public int salePrice = -1;
}
