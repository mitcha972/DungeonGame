using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class InventoryItemAmount
{
    public Item item;
    [Min(1)] public int count = 1;
}

public class Inventory : MonoBehaviour 
{
    public InventoryUI inventoryUI;

    // an item gets added/removed.
	public delegate void OnItemChanged();
	public OnItemChanged onItemChangedCallback;

	//Test Code//
	public delegate void OnItemAdd();
	public OnItemAdd onItemAddCallback;

    // Current list of items in inventory
    public List<Item> items = new List<Item>(20);
    [SerializeField, Min(1)] private int slotCount = 20;

    private void Awake()
    {
        if (items == null) items = new List<Item>();
        if (inventoryUI != null && inventoryUI.itemsParent != null)
        {
            inventoryUI.slots = inventoryUI.itemsParent.GetComponentsInChildren<InventorySlot>(true);
            slotCount = inventoryUI.slots.Length;
        }
        while (items.Count < slotCount) items.Add(null);
    }

    public bool Add(Item item)
    {
        if (item == null) return false;
        int slot = items.FindIndex(existing => existing == null);
        if (slot < 0) return false;
        items[slot] = item;
        RefreshSlot(slot);
        onItemChangedCallback?.Invoke();
        return true;
    }

    public void Remove(int itemSlot)
    {
        if (!ValidSlot(itemSlot)) return;
        items[itemSlot] = null;
        RefreshSlot(itemSlot);
        onItemChangedCallback?.Invoke();
    }

    public void Swap(Item newItem, int oldItemSlot)
    {
        if (!ValidSlot(oldItemSlot)) return;
        items[oldItemSlot] = newItem;
        RefreshSlot(oldItemSlot);
        onItemChangedCallback?.Invoke();
    }

    public void AddToSlot(Item item, int slot)
    {
        if (!ValidSlot(slot)) return;
        items[slot] = item;
        RefreshSlot(slot);
        onItemAddCallback?.Invoke();
        onItemChangedCallback?.Invoke();
    }

    public void SwapSlots(int first, int second)
    {
        if (!ValidSlot(first) || !ValidSlot(second) || first == second) return;
        Item previous = items[first];
        items[first] = items[second];
        items[second] = previous;
        RefreshSlot(first);
        RefreshSlot(second);
        onItemChangedCallback?.Invoke();
    }

    public int Count(Item item) => item == null ? 0 : items.FindAll(value => value == item).Count;

    private bool BuildExchange(IEnumerable<InventoryItemAmount> removals, IEnumerable<InventoryItemAmount> additions, out List<Item> result)
    {
        result = new List<Item>(items);
        foreach (var removal in removals)
        {
            if (removal == null || removal.item == null || removal.count < 1 || removal.count > result.Count) return false;
            for (int i = 0; i < removal.count; i++)
            {
                int slot = result.IndexOf(removal.item);
                if (slot < 0) return false;
                result[slot] = null;
            }
        }
        foreach (var addition in additions)
        {
            if (addition == null || addition.item == null || addition.count < 1 || addition.count > result.Count) return false;
            for (int i = 0; i < addition.count; i++)
            {
                int slot = result.IndexOf(null);
                if (slot < 0) return false;
                result[slot] = addition.item;
            }
        }
        return true;
    }
    public bool CanExchange(IEnumerable<InventoryItemAmount> removals, IEnumerable<InventoryItemAmount> additions)
        => BuildExchange(removals, additions, out _);
    public bool TryExchange(IEnumerable<InventoryItemAmount> removals, IEnumerable<InventoryItemAmount> additions, bool notify = true)
    {
        if (!BuildExchange(removals, additions, out var result)) return false;
        for (int i = 0; i < items.Count; i++) items[i] = result[i];
        if (notify) NotifyExchange();
        return true;
    }
    public void NotifyExchange()
    {
        for (int i = 0; i < items.Count; i++) RefreshSlot(i);
        onItemChangedCallback?.Invoke();
    }

    private bool ValidSlot(int slot) => slot >= 0 && slot < items.Count;
    private void RefreshSlot(int slot)
    {
        if (inventoryUI == null || inventoryUI.slots == null || slot >= inventoryUI.slots.Length || inventoryUI.slots[slot] == null) return;
        if (items[slot] == null) inventoryUI.slots[slot].ClearSlot(); else inventoryUI.slots[slot].AddItem(items[slot]);
    }
}
