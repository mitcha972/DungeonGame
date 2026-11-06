using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

//Controls the Inventory Slots Buttons 
public class InventorySlot : UISlot
{	
	//Change all to Awake()
	void Start()
	{
		equipmentManager = transform.parent.parent.GetComponent<InventoryUI>().owner.GetComponent<EquipmentManager>();

		//RIght click is item elsect
		transform.GetComponent<Button_UI>().ClickFunc = () => ItemSelect();

		//Left click is RemoveItem
		transform.GetComponent<Button_UI>().MouseRightClickFunc = () => RemoveFromInventory();

		//transform.GetComponent<Button_UI>().MouseOutOnceTooltipFunc = () => Tooltip_ItemStats.HideTooltip_Static();
        //transform.GetComponent<Button_UI>().MouseOverOnceTooltipFunc = () => Tooltip_ItemStats.ShowTooltip_Static(item);
	}

	//Set Up Function to check if still on tool tip onece activiated
	//Ienumater CheckMouseHover
	//{	
	//	While mouse is over = true
	//}
	
	public override void ItemSelect()
	{
		//When An Item Is pressed in the Inventory
		base.ItemSelect();
	}
	
}
