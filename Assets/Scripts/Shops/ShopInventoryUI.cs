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
		//inventory.onItemChangedCallback += UpdateUI;	// Subscribe to the onItemChanged callback

		// Populate our slots array
		slots = itemsParent.GetComponentsInChildren<ShopInventorySlot>(true);
	}

    public void OpenShop(ShopScript shop, PlayerControlState state)
    {
        if (state != null) _controlState = state;
        // Activate first so Awake initializes slots even on an initially inactive ShopUI.
        gameObject.SetActive(true);
        currentShop = shop;
        inventory = shop.inventory;
        for (int i = 0; i < slots.Length; i++)
        {
            bool stocked = i < inventory.items.Count && inventory.items[i] != null;
            slots[i].ClearSlot();
            slots[i].gameObject.SetActive(stocked);
            if (stocked) slots[i].AddItem(inventory.items[i]);
        }
        if (_controlState != null) _controlState.Synchronize();
        var button = GetComponentInChildren<UnityEngine.UI.Button>();
        if (button != null && UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
	
	[SerializeField] private PlayerControlState _controlState;
void Update() 
{
	    if (_controlState != null) return;
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
		

		//inventory.items.Clear();
		currentShop = null;
		inventory = null;

		//Open Character UI
		this.gameObject.SetActive(false);
        if (_controlState != null) _controlState.Synchronize();
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
