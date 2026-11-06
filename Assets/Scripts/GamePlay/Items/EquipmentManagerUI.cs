using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used by ???
public class EquipmentManagerUI : EquipmentManager 
{    
	// public override void Setup ()
	// {
	// 	int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
	// 	currentEquipment = new Equipment[numSlots];
    //     currentMeshes = new SkinnedMeshRenderer[numSlots];
	// }
	
	// public void Equip(Equipment newItem)
	// {
	// 	Debug.Log("Equipment Slot __ " + newItem.equipSlot);

	// 	// Find slot item is replacing
	// 	int slotIndex = (int)newItem.equipSlot;

	// 	//Unequip item in slot
    //     Equipment oldItem = Unequip(slotIndex);
		
	// 	if(onEquipmentChanged != null)
	// 		onEquipmentChanged.Invoke(newItem, oldItem);
		
	// 	//Equip new item to equiptment slot 
	// 	currentEquipment[slotIndex] = newItem;

	// 	AttachToMesh(newItem, slotIndex);
	// }

	// // Unequip an item with a particular index
	// public override Equipment Unequip (int slotIndex) //Add Switch Function
	// {
    //     Equipment oldItem = null;

	// 	// If Item is in current equiptment slot
	// 	if(currentEquipment[slotIndex] != null)
	// 	{
	// 		// Add the item to the inventory
	// 		oldItem = currentEquipment[slotIndex];

	// 		//Switch item with new 
	// 		//inventory[olditemslot] = oldItem);
	// 		//inventory.Add(oldItem);

	// 		// Remove the item from the equipment array
	// 		currentEquipment[slotIndex] = null;

    //         SetBlendShapeWeight(oldItem, 0);
    //         // Destroy the mesh
    //         if (currentMeshes[slotIndex] != null)
    //         {
    //             Destroy(currentMeshes[slotIndex].gameObject);
    //         }

	// 	}

    //     return oldItem;
	// }
	
}
