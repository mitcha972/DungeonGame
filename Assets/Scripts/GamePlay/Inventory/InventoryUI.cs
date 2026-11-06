using UnityEngine;
using System.Collections;

/* This object updates the inventory UI. */
public class InventoryUI : MonoBehaviour 
{
	public GameObject owner; //palyer using Ui
	public Transform itemsParent;	// The parent object of all the items
	public GameObject inventoryUI;	// The entire UI
	public Inventory inventory;	// Our current inventory //owner.inventory
	public InventorySlot[] slots;	// List of all the slots

	void Start()
	{
		//inventory.onItemAddCallback += UIAdd;
		//inventory.onItemRemoveCallback += UIAdd;

		// Populate our slots array
		slots = itemsParent.GetComponentsInChildren<InventorySlot>();
	}

	void UIAdd()
	{
		//Add Item To Ui
		StartCoroutine(UpdateUI());
	} 

	void UIUpdate()
	{
		//Add Item To Ui
		//StartCoroutine(UpdateUI());
	}  

	void UIRemove(int Int)
	{
		//Remove Item From UI
		//StartCoroutine(UpdateUIDelete(Int));
	}  

	//Add item to slot
	public IEnumerator UpdateUI()
	{		
		yield return new WaitForSeconds(0.01f);

		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].item == null) 
			{
				//inventory.items[inventory.items.Count -1].ISN = i;
				//slots[i].AddItem(inventory.items[inventory.items.Count -1]);
				break;
			}
		}

	}	
}
