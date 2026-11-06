using UnityEngine;

/* This object updates the inventory UI. */

public class ShopInventoryUI : MonoBehaviour {

	public Transform itemsParent;	// The parent object of all the items
	//public GameObject inventoryUI;	// The entire UI

	public ShopInventory inventory;	// Our current inventory

	public ShopInventorySlot[] slots;	// List of all the slots

	public GameObject[] inventoryItems; 

	public ShopScript currentShop; 

	void Awake () 
	{
		inventory = ShopInventory.instance;
		//inventory.onItemChangedCallback += UpdateUI;	// Subscribe to the onItemChanged callback

		// Populate our slots array
		slots = itemsParent.GetComponentsInChildren<ShopInventorySlot>();
	}
	
	void Update() 
	{
		// Check to see if we should open/close the inventory
		if (Input.GetButtonDown("Cancel"))
			CloseUI();
	}

	public void CloseUI() 
	{
		//Remove all Items
		for(int i = 0; i < slots.Length; i++)
		{
			//Debug.Log("Clared - " + i);
			slots[i].ClearSlot();				
		}				
		
		slots = null;
		slots = itemsParent.GetComponentsInChildren<ShopInventorySlot>();
		//inventory.items.Clear();
		currentShop = null;
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
