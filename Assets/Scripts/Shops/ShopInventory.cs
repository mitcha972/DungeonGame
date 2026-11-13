using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopInventory : MonoBehaviour
{
    public static ShopInventory instance;
    public int space = 9;
    public List<Item> items = new List<Item>();
    public ShopDefinition definition;
    private readonly List<int> quantities = new List<int>();
    private bool initialized, trading;
    private double nextRestock;
    public event Action Changed;
    void Awake() { if (instance == null) instance = this; }
    void OnDestroy() { if (instance == this) instance = null; }
    void Update() => Restock(Time.timeAsDouble);
    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        var legacy = new List<Item>(items); items.Clear(); quantities.Clear();
        if (definition != null)
        {
            foreach (var r in definition.stock)
                if (r != null && r.item != null && !items.Contains(r.item) && items.Count < 9)
                { items.Add(r.item); quantities.Add(Mathf.Clamp(r.initial, 0, Mathf.Max(1, r.maximum))); }
        }
        else foreach (var item in legacy) Add(item);
        nextRestock = Time.timeAsDouble + (definition != null ? Math.Max(0, definition.restockInterval) : 0);
    }
    public int Quantity(Item item) { Initialize(); int i = items.IndexOf(item); return i < 0 ? 0 : quantities[i]; }
    public int Maximum(Item item) => definition == null ? 10 : Mathf.Max(1, definition.Rule(item)?.maximum ?? definition.purchasedItemCapacity);
    public bool Add(Item item)
    {
        Initialize(); if (item == null) return false;
        int i = items.IndexOf(item);
        if (i < 0) { if (items.Count >= 9) return false; items.Add(item); quantities.Add(0); i = items.Count - 1; }
        if (quantities[i] >= Maximum(item)) return false;
        quantities[i]++; Changed?.Invoke(); return true;
    }
    public void Remove(Item item) { Initialize(); int i = items.IndexOf(item); if (i >= 0 && quantities[i] > 0) { quantities[i]--; Changed?.Invoke(); } }
    public void Restock(double now)
    {
        Initialize(); if (definition == null || definition.restockInterval <= 0 || now < nextRestock) return;
        double ticks = Math.Floor((now - nextRestock) / definition.restockInterval) + 1;
        nextRestock += ticks * definition.restockInterval;
        bool changed = false;
        for (int i = 0; i < items.Count; i++)
        {
            var rule = definition.Rule(items[i]); if (rule == null) continue;
            int target = (int)Math.Min(Maximum(items[i]), quantities[i] + ticks * Math.Max(0, rule.restockAmount));
            changed |= target != quantities[i]; quantities[i] = target;
        }
        if (changed) Changed?.Invoke();
    }
    public int Price(Item item, bool buying) => definition != null ? definition.Price(item, buying) : Mathf.Max(0, item.price);
    public string Refusal(Item item)
    {
        Initialize();
        if (definition == null || !definition.Accepts(item)) return "This merchant does not buy that item.";
        if (!items.Contains(item) && items.Count >= 9) return "Merchant has no free stock slot.";
        if (Quantity(item) >= Maximum(item)) return "Merchant stock is full.";
        return null;
    }
    public bool Trade(Inventory player, Item item, bool buying, out string feedback)
    {
        feedback = "Trade unavailable.";
        if (trading || player == null || item == null) return false;
        Initialize();
        if (buying && (!items.Contains(item) || Quantity(item) < 1 || (definition != null && !definition.Sells(item)))) { feedback = "Out of stock or unavailable."; return false; }
        if (!buying) { var refusal = Refusal(item); if (refusal != null) { feedback = refusal; return false; } }
        int price = Price(item, buying);
        if (buying && player.Gold < price) { feedback = "Not enough gold."; return false; }
        if (!buying && player.Gold > int.MaxValue - price) { feedback = "Gold limit reached."; return false; }
        var amount = new[] { new InventoryItemAmount { item = item, count = 1 } };
        var empty = Array.Empty<InventoryItemAmount>();
        trading = true;
        try
        {
            if (!player.TryExchange(buying ? empty : amount, buying ? amount : empty, false)) { feedback = buying ? "Inventory is full." : "Item is no longer in your bag."; return false; }
            player.SetGoldAfterTrade(buying ? player.Gold - price : player.Gold + price);
            if (buying) quantities[items.IndexOf(item)]--;
            else { int i = items.IndexOf(item); if (i < 0) { items.Add(item); quantities.Add(1); } else quantities[i]++; }
            feedback = (buying ? "Bought " : "Sold ") + item.name + " for " + price + " gold.";
            player.NotifyExchange(); Changed?.Invoke(); return true;
        }
        finally { trading = false; }
    }
}
