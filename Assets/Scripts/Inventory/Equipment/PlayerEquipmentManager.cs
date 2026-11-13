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
        animator = GetComponent<CharacterAnimator>();
	}

	public override void Setup()
	{
		//...Do something
	}

	// Equip a New Item
	public override void Equip (Equipment newItem, int SlotIndex)
	{
        if (newItem == null || inventory == null || SlotIndex < 0 || SlotIndex >= inventory.items.Count || inventory.items[SlotIndex] != newItem) return;
		//Check if free Animation
		if (animator != null && animator.AnimationIsBusy())
		 	return;

		base.Equip(newItem, SlotIndex);
		
		AddToUI(newItem);
		if (animator != null) animator.PlayTargetAnimation("Equip", true);							
	}

	public override void Equip(Equipment newItem)
	{
        if (newItem == null) return;
		base.Equip(newItem);
		AddToUI(newItem);
	}

    public override Equipment Unequip(EquipmentSlot slotIndex)
    {
        Equipment removed = base.Unequip(slotIndex);
        if (equipmentSlotsUI != null)
            foreach (var slot in equipmentSlotsUI)
                if (slot != null && slot.equipmentSlot == slotIndex) slot.ClearSlot();
        return removed;
    }

    void AddToUI(Equipment newItem)
	{
		//Add Item image To equiptment Slot UI
		if (equipmentSlotsUI == null) return;
        for(int i = 0; i < equipmentSlotsUI.Length; i++)
			if(equipmentSlotsUI[i] != null && equipmentSlotsUI[i].equipmentSlot == newItem.equipSlot)
				equipmentSlotsUI[i].AddItem(newItem);
	}

	public void RemoveFromInventory() //remove item from inventory //TODO
	{
		//Remove from inventory
		//inventory.Remove(this.transform.GetSiblingIndex());		
	}
}
