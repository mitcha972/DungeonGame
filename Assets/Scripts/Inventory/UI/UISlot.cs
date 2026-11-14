using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        var button = UIPointerEvents.GetOrAdd(this);
        if (button != null)
        {
            button.Exited = () => Tooltip_ItemStats.HideTooltip_Static();
            button.Entered = () => Tooltip_ItemStats.ShowTooltip_Static(item);
        }
	}

	public virtual void ItemSelect()//Change to slotpressed//
	{
		//check if busy
        if (item == null || inventory == null) return;
        CharacterAnimator ani = equipmentManager != null ? equipmentManager.GetComponentInParent<CharacterAnimator>() : null;

        if (ani != null && ani.AnimationIsBusy())
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
		if (newItem == null) { ClearSlot(); return; }
		
		item = newItem;
        if (icon == null) return;
		icon.sprite = item.icon;
		icon.enabled = true;
		//removeButton.interactable = true;
	}

	public void ClearSlot()
	{
		// clear the slot
		//Debug.Log(slotIndex +  "- Slot Cleared..." );
		
		item = null;
        if (icon == null) return;
		icon.sprite = null;
		icon.enabled = false;
		//removeButton.interactable = false;
	}

	public void RemoveFromInventory()//Change to Drop Item in World
	{
		// Called when the remove button is pressed

		if (item == null || inventory == null || equipmentManager == null)
			return;
        if (slotIndex < 0 || slotIndex >= inventory.items.Count || inventory.items[slotIndex] != item) return;

		//Add Item Back to world
		//Change//
		if (!equipmentManager.TryDropItem(item)) return;
		inventory.Remove(slotIndex); 
		ClearSlot();
	}	

	public void OnDrop(PointerEventData eventData)
    {
        if (!(this is InventorySlot) || eventData.pointerDrag == null) return;
        var drag = eventData.pointerDrag.GetComponent<DragDrop>();
        if (drag == null || drag.inventorySlot == null || inventory == null || drag.inventorySlot.inventory != inventory) return;
        inventory.SwapSlots(transform.GetSiblingIndex(), drag.inventorySlot.transform.GetSiblingIndex());
    }

}
