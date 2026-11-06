using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum EquipmentSlot { Head, Chest, Legs, MainHand, OffHand, Feet}
public enum EquipmentSlot { Head, Chest, Legs, Feet, MainHand, OffHand}

[CreateAssetMenu(fileName = "New Equipment", menuName = "Item/Equipment")]
public class Equipment : Item 
{
	[Header("Equipment")]
	public EquipmentSlot equipSlot;	// Slot to store equipment in
	public SkinnedMeshRenderer mesh;
    public EquipmentManager.MeshBlendShape[] coveredMeshRegions;

	//Fix chage to dictonary of names and ints
	[Header("Modifiers")]
	public int armorModifier;		// Increase/decrease in armor
	public int damageModifier;      // Increase/decrease in damage
	public int rangeModifier;      // Range in damage  

	public override void Use(GameObject owner, int SlotIndex)
	{
		// When Item is pressed in inventory Equip
		Equip(owner, SlotIndex);
	}

	public void Equip(GameObject owner, int slotIndex)
	{
		//Change to switch state based on the what the item is:
		// if(itemType == ItemType.Potion)//"Potion")
		// {
		// 	//find player
		// 	PlayerStats PS = owner.GetComponent<PlayerStats>();
			
		// 	Debug.Log("Heal For - " + modifier);

		// 	//Add health
		// 	if(PS.currentHealth + modifier >= PS.maxHealth)
		// 		PS.currentHealth = PS.maxHealth;
		// 	else
		// 		PS.currentHealth += modifier;

		// 	PS.Heal(modifier);

		// 	return;
		// }

		//Find equipmentManager And Equip Item 
		EquipmentManager equipmentManager = owner.GetComponent<EquipmentManager>();
		equipmentManager.Equip(this, slotIndex);
	}

}
