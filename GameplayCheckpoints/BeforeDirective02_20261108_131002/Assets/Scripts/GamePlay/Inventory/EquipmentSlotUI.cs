using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

//Used by the ??
public class EquipmentSlotUI : UISlot 
{
	public EquipmentSlot equipmentSlot;

	void Start()
	{
		//Left Click 
		transform.GetComponent<Button_UI>().ClickFunc = () => ItemSelect();

		//Right Click
		//transform.GetComponent<Button_UI>().MouseRightClickFunc = () => RemoveFromInventory();

		//equipmentManager = transform.parent.parent.GetComponent<InventoryUI>().owner.GetComponent<EquipmentManager>();
		//transform.GetComponent<Button_UI>().MouseOutOnceTooltipFunc = () => Tooltip_ItemStats.HideTooltip_Static();
        //transform.GetComponent<Button_UI>().MouseOverOnceTooltipFunc = () => Tooltip_ItemStats.ShowTooltip_Static(item);
	}

	// Called when the item is pressed
	public override void ItemSelect()
	{		
		if (item != null)
		{
			//Check equipment slot , if there is a curent item then unequpt it. 
        	Equipment oldItem = equipmentManager.Unequip(equipmentSlot);

			//Add Item To Inventory
			inventory.Add(item);			

			//Clear Slot
			ClearSlot();			
		}
	}

	public void Unequip()//Fix
	{		
		Debug.Log("Item UnEquipted -- " );
		
		//remove item from slot //Add BACKKKK!
		//if(equipmentManager.onEquipmentChanged != null)
		//	equipmentManager.onEquipmentChanged.Invoke(null, equipmentManager.currentEquipment[transform.GetSiblingIndex()]);

		//Drop Item
		//equipmentManager.Unequip(transform.GetSiblingIndex());//use to be UnEquip
		//equipmentManager.DropItem(item);
		
		ClearSlot();
				
		//Equip new item to equiptment slot 
		//remove item
		//
	}
}
