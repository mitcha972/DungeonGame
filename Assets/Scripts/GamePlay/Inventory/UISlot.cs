using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEngine.EventSystems;

/* Sits on all InventorySlots. Used By (InventorySlot, EquipmentSlotUI, ChestIventorySlot) */ 
public class UISlot : MonoBehaviour , IDropHandler 
{
	public Item item;  
	public Image icon;	//Make Private		
	public Button removeButton;
	public int slotIndex;
	
	public Inventory inventory;
	public EquipmentManager equipmentManager;
	
	// Add item to the slot
	void Awake()
	{
		slotIndex = transform.GetSiblingIndex();
		//icon = transform.Find("icon").gameObject.Image;
		transform.name = "slot" + slotIndex;

		//Tool Tip
		transform.GetComponent<Button_UI>().MouseOutOnceTooltipFunc = () => Tooltip_ItemStats.HideTooltip_Static();
        transform.GetComponent<Button_UI>().MouseOverOnceTooltipFunc = () => Tooltip_ItemStats.ShowTooltip_Static(item);
	}

	public virtual void ItemSelect()//Change to slotpressed//
	{
		//check if busy
		CharacterAnimator ani = equipmentManager.gameObject.GetComponent<CharacterAnimator>();

		if(ani.AnimationIsBusy())
			return;

		// Called when the item is pressed in inventory
		if (item == null)
			return;

		//Use the Item
		item.Use(inventory.gameObject, slotIndex);

		//If item is potion
		// if(item.itemType == ItemType.Potion)
		// {
		// 	inventory.Remove(slotIndex); 
		// 	ClearSlot();
		// }
		
	}

	public void AddItem (Item newItem) //add item to slot
	{
		Debug.Log(newItem + " - Addedededede to slot: " +slotIndex );
		
		item = newItem;
		icon.sprite = item.icon;
		icon.enabled = true;
		//removeButton.interactable = true;
	}

	public void ClearSlot()
	{
		// clear the slot
		//Debug.Log(slotIndex +  "- Slot Cleared..." );
		
		item = null;
		icon.sprite = null;
		icon.enabled = false;
		//removeButton.interactable = false;
	}

	public void RemoveFromInventory()//Change to Drop Item in World
	{
		// Called when the remove button is pressed

		if (item == null)
			return;

		Debug.Log("Drop Item");

		//Add Item Back to world
		//Change//
		equipmentManager.DropItem(item);
		inventory.Remove(slotIndex); 
		ClearSlot();
	}	

	public void OnDrop(PointerEventData eventData)//Change to - Drop Item In Slot 
	{
		//Drop Item In Slot 
		
		//Find Slot being dragged
		if(eventData.pointerDrag)
		{
			//When Dropping Moving slot
			Debug.Log("Droped on Slot -- " + this.gameObject.name);
			//Slot being dragged
			InventorySlot inventorySlot = eventData.pointerDrag.GetComponent<DragDrop>().inventorySlot;
			
			//item in drop slot 
			Item tempItem = item;

			//item being moved 
			Item moveItem = inventorySlot.item;

			//Change to inventoy
			int firstS = this.transform.GetSiblingIndex();
			Item II = inventory.items[firstS];
			int secondS = inventorySlot.transform.GetSiblingIndex();

			inventory.items[firstS] = inventory.items[secondS];
			inventory.items[secondS] = II;
			//inventory.Remove(this.transform.GetSiblingIndex());
			//inventory.Remove(inventorySlot.transform.GetSiblingIndex());
			
			inventorySlot.ClearSlot();
			ClearSlot();

			//item in drop slot 
			if(tempItem != null)
			{
				Debug.Log("Item IN dropppp");
				//tempItem.ISN = retempInt;//1
				//inventory.Add(tempItem);
				inventorySlot.AddItem(tempItem);
			}

			AddItem(moveItem);
			//inventory.Add(moveItem);

			//inventory.SwapItem();
		}		
	}

}
