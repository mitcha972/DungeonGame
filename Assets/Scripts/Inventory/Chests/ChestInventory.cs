using System.Collections.Generic;
using UnityEngine;

public class ChestInventory : MonoBehaviour
{
    public static ChestInventory instance;
    public int space = 4;
    public List<Item> items = new List<Item>();

    private void Awake()
    {
        if (instance == null) instance = this;
        if (items == null) items = new List<Item>();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public bool Add(Item item)
    {
        if (item == null || items.Count >= space) return false;
        items.Add(item);
        return true;
    }

    public void Remove(Item item)
    {
        items.Remove(item);
    }
}
