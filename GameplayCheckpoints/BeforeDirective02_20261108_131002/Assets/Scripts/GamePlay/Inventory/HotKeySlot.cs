using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/* Sits on all InventorySlots. */
public class HotKeySlot : UISlot
{
	public InventorySlot inventorySlot;
	public KeyCode keyCode;

	void Update()
	{
		if(Input.GetKeyDown(keyCode) )
		{
			if (inventorySlot.item == null)
				return;

			Debug.Log("Keyyyyy Clicked");
			inventorySlot.ItemSelect();
			//base.ItemSelect();
		}
	}

/*
	public void UseItem()
	{
		//Debug.Log("Use dus BICHHHHH");
		InventorySlot.UseItem();
	}

*/

}
