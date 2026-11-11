using UnityEngine;

/* This object updates the inventory UI. */

public class ChestInventoryUI : MonoBehaviour {

	public Transform itemsParent;	// The parent object of all the items
	//public GameObject inventoryUI;	// The entire UI

	public ChestInventory inventory;	// Our current inventory

	public ChestInventorySlot[] slots;	// List of all the slots

	public GameObject[] inventoryItems; 

	public ChestScript currentChest; 

	void Awake () 
	{
		inventory = ChestInventory.instance;
		//inventory.onItemChangedCallback += UpdateUI;	// Subscribe to the onItemChanged callback

		// Populate our slots array
		slots = itemsParent.GetComponentsInChildren<ChestInventorySlot>(true);
		this.transform.gameObject.SetActive(false);
	}
	
	public void CloseUI() 
	{
		//Remove all Items
		for(int i = 0; i < slots.Length; i++)
		{
			//Debug.Log("Clared - " + i);
			slots[i].ClearSlot();				
		}				
		

		//inventory.items.Clear();
		currentChest = null;
		inventory = null;

		//Open Character UI
		this.gameObject.SetActive(false);
	}

	// Update the inventory UI by:
	//		- Adding items
	//		- Clearing empty slots
	// This is called using a delegate on the Inventory.
	void UpdateUI ()
	{
		// Loop through all the slots
		for (int i = 0; i < slots.Length; i++)
		{
			if (i < inventory.items.Count)	// If there is an item to add
			{
				slots[i].AddItem(inventory.items[i]);	// Add it
			} else
			{
				// Otherwise clear the slot
				slots[i].ClearSlot();
			}
		}
	}

	public void AddItems()
	{
		//Add Items
		//slots[0].AddItem(inventoryItems[0].GetComponent<ItemPickup>().item);
	}
}
