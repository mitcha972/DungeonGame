using System;
using System.Collections.Generic;
using UnityEngine;

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

    public bool Add(Item item)
    {			
        // Add item to Inventory If there is enough room
        for (int i = 0; i < items.Count; i++)
        {
            //If there is not an item is the current slot add
            if(items[i] == null)
            {
                //Add item to inventory
                items[i] = item;

                //Add item to UI inventory Slot
                inventoryUI.slots[i].AddItem(item);

                // Trigger Add Item
				if (onItemChangedCallback != null)
					onItemChangedCallback.Invoke();

                return true;
            }		
        }
		//Cant Add An Item 
        return false;		
    }

    public void Remove(int itemSlot) 
    {		
        // Remove item from Inventory
        items[itemSlot] = null;
        inventoryUI.slots[itemSlot].ClearSlot();

        	// Trigger callback			
		if (onItemChangedCallback != null)
			onItemChangedCallback.Invoke();	
    }

    public void Swap(Item newItem, int oldItemSlot)
    {
        //Called when items are swapped from Inventory and Equip Slot
        Debug.Log("Item - " + newItem + " - Swaped in Slot: " + oldItemSlot);
		

        //Update items list
        items[oldItemSlot] = newItem;

        //Update UI slot
		//

		//
        inventoryUI.slots[oldItemSlot].AddItem(newItem);
    }

    public void AddToSlot(Item item, int slot)
    {	
		//
        items[slot] = item;
        inventoryUI.slots[slot].AddItem(item);

        // Trigger Add Item
		if (onItemAddCallback != null)
			onItemAddCallback.Invoke();
    }
}
