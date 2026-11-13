using UnityEngine;
using UnityEngine.UI;

/* Sits on all InventorySlots. */

public class ShopInventorySlot : MonoBehaviour {

	public Image icon;			// Reference to the Icon image
	//public Button removeButton;	// Reference to the remove button
	public InventoryUI IU;
	public ShopInventoryUI CUI;

	public Text itemPriceText;

	Item item;  // Current item in the slot

	void Start()
	{
		if(!item)
			gameObject.SetActive(false);
	}

	// Add item to the slot
	public void AddItem (Item newItem)
	{
		item = newItem;

		icon.sprite = item.icon;
		icon.enabled = true;

		itemPriceText.text = newItem.price.ToString();
		//removeButton.interactable = true;
	}

	// Clear the slot
	public void ClearSlot()
	{
		item = null;

		icon.sprite = null;
		icon.enabled = false;
		//removeButton.interactable = false;
	}	

	// Called when the item is pressed
	public void UseItem ()
	{		
		if (item != null)
		{
			//IU.slots[IU.inventory.items.Count].AddItem(item);
			//Add Itme to inventory
			IU.inventory.Add(item);			

			//Remove from Shop Inventory
			//CUI.currentShop.inventory.Remove(item);

			//Clear Slot
			//ClearSlot();			

			//item.Use();
		}
	}

}
