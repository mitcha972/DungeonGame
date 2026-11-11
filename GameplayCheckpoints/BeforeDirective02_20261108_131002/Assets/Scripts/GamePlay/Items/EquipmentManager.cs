using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Keep track of equipment. Has functions for adding and removing items. */
public class EquipmentManager : MonoBehaviour //used by PEM EEM
{
    // Mesh blend shapes used for equipment attachments
	public enum MeshBlendShape { Torso, Arms, Legs , Feet};
	
	// Equipment slot names used in the game
	public enum EquipmentSlotNames{Head, Chest, Legs, Feet, MainHand, OffHand} // Change to use Equipments Enum

    // Dictionary to map equipment slots to their corresponding names
    public Dictionary<EquipmentSlot, string> slotNames = new Dictionary<EquipmentSlot, string>
    {
		{ EquipmentSlot.Head, "Head" },
        { EquipmentSlot.Chest, "Chest" },
        { EquipmentSlot.Legs, "Legs" },
		{ EquipmentSlot.Feet, "Feet" },
        { EquipmentSlot.MainHand, "MainHand" },
		{ EquipmentSlot.OffHand, "OffHand" }
		//{ EquipmentSlot.MainHand, "MainHand" }

    };

    public Dictionary<EquipmentSlot, Equipment> currentEquipment = new Dictionary<EquipmentSlot, Equipment>();

	// Public variables used by the EquipmentManager
	public SkinnedMeshRenderer targetMesh;
	public Dictionary<EquipmentSlot, SkinnedMeshRenderer> currentMeshes = new Dictionary<EquipmentSlot, SkinnedMeshRenderer>();
   
	public GameObject itemDrop;
	public GameObject attackHitBox;

	// Callback for when an item is equipped/unequipped//Equipment Swap // Used By ...?
	public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
	public OnEquipmentChanged onEquipmentChanged;

	// Private variables used by the EquipmentManager
	public Inventory inventory;

	// Array to keep track of the current equipment
    public Equipment[] currentEquipmentArray = new Equipment[System.Enum.GetValues(typeof(EquipmentSlot)).Length];
	
	//Aimator
	public CharacterAnimator animator;

	void Start()
	{
		animator = GetComponent<CharacterAnimator>();
		Setup();
	}

	// Update the currentEquipmentArray whenever the currentEquipment dictionary is updated
    void UpdateCurrentEquipmentArray()
    {
        for (int i = 0; i < currentEquipmentArray.Length; i++)
        {
            currentEquipmentArray[i] = currentEquipment.TryGetValue((EquipmentSlot)i, out Equipment equipment) ? equipment : null;
        }
    }

	public virtual void Setup()
	{
		//...Do somthing for enwmy and charcter
	}
	
	public virtual void Equip(Equipment newItem, int SlotIndex)
	{
		// Set equip trigger
    	animator.Equip();

		Debug.Log("Equipping " + newItem.name + "From Index-" +SlotIndex + " to " + slotNames[newItem.equipSlot]);

		// Find equipment slot item is replacing
		EquipmentSlot slotIndex = newItem.equipSlot;

		//check equipment slot , if there is a curent item then unequip it. 
		Equipment oldItem = Unequip(slotIndex, SlotIndex, newItem);

		//Equip new item to equiptment slot 
		currentEquipment[slotIndex] = newItem;
		
		//Add update eupiment ui Here
		UpdateCurrentEquipmentArray();

		AttachToMesh(newItem, slotIndex);

		//Attak Hit box function
		//Set AttackHitBox parent To hand GameObject object based on if the Equipted slot is a "MainHand"
		// if(newItem.equipSlot == EquipmentSlot.MainHand)
		// {
		// 	attackHitBox.GetComponent<AttackHitBox>().UpdateAttackHitBox(newItem);
		// }
			
	}

	public virtual void Equip(Equipment newItem)
	{
		// Set equip trigger
    	animator.Equip();

		Debug.Log("Equipping " + newItem.name + " to " + slotNames[newItem.equipSlot]);

		// Find equipment slot item is replacing
		EquipmentSlot  slotIndex = newItem.equipSlot;

		//check equipment slot , if there is a curent item then unequpt it. 
        Equipment oldItem = Unequip(slotIndex);
		
		//FIXXXX
		if(onEquipmentChanged != null)
			onEquipmentChanged.Invoke(newItem, oldItem);
		
		//Equip new item to equiptment slot 
		currentEquipment[slotIndex] = newItem;
		UpdateCurrentEquipmentArray();

		AttachToMesh(newItem, slotIndex);

		//Set AttackHitBox parent To hand GameObject object based on if the Equipted slot is a "MainHand"
		// if(newItem.equipSlot == EquipmentSlot.MainHand)
		// {
		// 	attackHitBox.GetComponent<AttackHitBox>().UpdateAttackHitBox(newItem);
		// }
	}

	public virtual Equipment Unequip(EquipmentSlot slotIndex, int SlotIndex, Equipment newItem ) //Add Switch Function
	{
        Equipment oldItem = null;
		
		// If Item is in current equiptment slot
		if(currentEquipment.ContainsKey(slotIndex))
		{
			// Find Item being removed from Eq
			oldItem = currentEquipment[slotIndex];
			
			Debug.Log("Unequipint - " + oldItem + "from Slot- " + SlotIndex);
	
			inventory.Swap(oldItem, SlotIndex);

			// Remove the item from the equipment dictionary
            currentEquipment.Remove(slotIndex);
			UpdateCurrentEquipmentArray();

            SetBlendShapeWeight(oldItem, 0);
           
		    // Destroy the mesh
            if (currentMeshes[slotIndex] != null)
                Destroy(currentMeshes[slotIndex].gameObject);
            
		}
		else
		{
			inventory.Remove(SlotIndex);
		}

		// Trigger the onEquipmentChanged callback : Called in - ??
		if (onEquipmentChanged != null)
			onEquipmentChanged.Invoke(newItem, oldItem);

        return oldItem;
	}

	public virtual Equipment Unequip(EquipmentSlot slotIndex) //Add Switch Function
	{
		Equipment oldItem = null;
		
		// If Item is in current equiptment slot
		if(currentEquipment.ContainsKey(slotIndex))
		{
			// Find Item being removed from Eq
			oldItem = currentEquipment[slotIndex];
			
			Debug.Log("Unequipint - " + oldItem);

			// Remove the item from the equipment dictionary
            currentEquipment.Remove(slotIndex);
			
			UpdateCurrentEquipmentArray();

            SetBlendShapeWeight(oldItem, 0);
           
		    // Destroy the mesh
            if (currentMeshes[slotIndex] != null)
                Destroy(currentMeshes[slotIndex].gameObject);

		}

		//FIX OR JUST REMOVE 
		if(onEquipmentChanged != null)
			onEquipmentChanged.Invoke(null, oldItem);

        return oldItem;
	}

	public void UnequipAll()
	{
		foreach (EquipmentSlot slot in currentEquipment.Keys.ToList())
		{
			Unequip(slot);
		}
	}

	public void DropItem(Item dropI)
	{
		var it = dropI as Equipment;
		
		///WOrkig on 
		GameObject newItemDrop = Instantiate(itemDrop,this.transform.position + this.transform.up + this.transform.forward, Quaternion.identity);
		newItemDrop.name = it.name;
		newItemDrop.GetComponent<ItemInteract>().item = dropI;
		newItemDrop.transform.Find("ItemPreview").GetComponent<MeshFilter>().sharedMesh = it.mesh.sharedMesh;
		newItemDrop.transform.Find("ItemPreview").GetComponent<MeshRenderer>().materials = it.mesh.sharedMaterials;

		//Change Item Size//
		newItemDrop.transform.Find("ItemPreview").GetComponent<Transform>().localScale = it.mesh.GetComponent<Transform>().localScale;
	
		//Remove from inventory
		//inventory.Remove(this.transform.GetSiblingIndex()); //fix	
		//RemoveFromInventory(item);
	}

    public void AttachToMesh(Equipment item, EquipmentSlot slotIndex)
	{
        SkinnedMeshRenderer newMesh = Instantiate(item.mesh) as SkinnedMeshRenderer;
        
		//Change to SetParent
		//newMesh.transform.parent = targetMesh.transform.parent;
		newMesh.transform.SetParent(targetMesh.transform.parent, false);

		if(item.equipSlot == EquipmentSlot.MainHand)
    	{
			//FIXX Change newmesh Boxcollider to int values.
			BoxCollider newCollider = newMesh.GetComponent<BoxCollider>();
			if (newCollider != null)
			{
				attackHitBox.GetComponent<BoxCollider>().center = newCollider.center;
				attackHitBox.GetComponent<BoxCollider>().size = newCollider.size;
			}
		}

		//Set Bones
        newMesh.rootBone = targetMesh.rootBone;
		newMesh.bones = targetMesh.bones;
		
		currentMeshes[slotIndex] = newMesh;

        SetBlendShapeWeight(item, 100);
       
	}

    public void SetBlendShapeWeight(Equipment item, int weight)
	{
		foreach (MeshBlendShape blendShape in item.coveredMeshRegions)
		{
			int shapeIndex = (int)blendShape;
			targetMesh.SetBlendShapeWeight(shapeIndex, weight);
		}
	}

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.U))
			UnequipAll();
	}
}
