using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipmentManager : EquipmentManager 
{
	//Player Equipment slots ui ( left side)
	public EquipmentSlotUI[] equipmentSlotsUI;
	
	void Awake()
	{
		inventory = GetComponent<Inventory>();
	}

	public override void Setup()
	{
		//...Do something
	}

	// Equip a New Item
	public override void Equip (Equipment newItem, int SlotIndex)
	{
		//Check if free Animation
		if (animator.AnimationIsBusy())
		 	return;

		base.Equip(newItem, SlotIndex);
		
		AddToUI(newItem);
		animator.PlayTargetAnimation("Equip", true);							
	}

	public override void Equip (Equipment newItem)
	{
		//Check if free Animation
		if (animator.AnimationIsBusy())
		 	return;

		base.Equip(newItem);
		
		//Add to slot
		//
		AddToUI(newItem);
		animator.PlayTargetAnimation("Equip", true);						
	}

	void AddToUI(Equipment newItem)
	{
		//Add Item image To equiptment Slot UI
		for(int i = 0; i < equipmentSlotsUI.Length; i++)
			if(equipmentSlotsUI[i].equipmentSlot == newItem.equipSlot)
				equipmentSlotsUI[i].AddItem(newItem);
	}

	public void RemoveFromInventory() //remove item from inventory //TODO
	{
		//Remove from inventory
		//inventory.Remove(this.transform.GetSiblingIndex());		
	}
}
