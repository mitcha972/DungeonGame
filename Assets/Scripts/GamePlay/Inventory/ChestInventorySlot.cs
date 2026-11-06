using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class ChestInventorySlot : UISlot 
{
	//public Button removeButton;	// Reference to the remove button
	public InventoryUI IU;
	public ChestInventoryUI CUI;

	void Start()
	{
		//Click Function
		transform.GetComponent<Button_UI>().ClickFunc = () => ItemSelect();
	}

	// Called when the item is pressed
	public override void ItemSelect()
	{		
		if (item != null)
		{
			//Add Itme to inventory
			IU.inventory.Add(item);			

			//Remove from Chest Inventory
			CUI.currentChest.inventory.Remove(item);

			//Clear Slot
			ClearSlot();		
		}
	}

}
