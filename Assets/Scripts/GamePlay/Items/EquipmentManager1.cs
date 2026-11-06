using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Keep track of equipment. Has functions for adding and removing items. */

public class EquipmentManager1 : MonoBehaviour {
	/*

	#region Singleton

    public enum MeshBlendShape {Torso, Arms, Legs };
    public Equipment[] defaultEquipment;

	public static EquipmentManager instance;
	public SkinnedMeshRenderer targetMesh;

    SkinnedMeshRenderer[] currentMeshes;

	public GameObject itemDrop;
	public Transform player;

	public EquipmentSlotUI[] ES;//Test Code

	void Awake ()
	{
		instance = this;
	}

	#endregion

	public Equipment[] currentEquipment;   // Items we currently have equipped

	// Callback for when an item is equipped/unequipped
	public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
	public OnEquipmentChanged onEquipmentChanged;
   

	Inventory inventory;	// Reference to our inventory

	void Start ()
	{
		inventory = Inventory.instance;		// Get a reference to our inventory

		// Initialize currentEquipment based on number of equipment slots
		int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
		currentEquipment = new Equipment[numSlots];
        currentMeshes = new SkinnedMeshRenderer[numSlots];

        EquipDefaults();
	}

	// Equip a new item
	public void Equip (Equipment newItem)
	{
		//Debug.Log("Equipment Slot __ " + newItem.equipSlot);
		// Find out what slot the item fits in
		int slotIndex = (int)newItem.equipSlot;

        Equipment oldItem = Unequip(slotIndex, true);

		// An item has been equipped so we trigger the callback
		if (onEquipmentChanged != null)
		{
			onEquipmentChanged.Invoke(newItem, oldItem);
		}

		//Add Item To equiptment Slot// Work in Progress
		for(int i = 0; i < ES.Length; i++)
		{
			//Debug.Log(i+ " - " + ES[i].equipSlot);
		
			if(ES[i].equipSlot == newItem.equipSlot)
			{
				ES[i].AddItem(newItem);
			}
		}
				

		// Insert the item into the slot
		currentEquipment[slotIndex] = newItem;
        AttachToMesh(newItem, slotIndex);
	}

	// Unequip an item with a particular index
	public Equipment Unequip (int slotIndex, bool switchItem) //Add Switch Function
	{
        Equipment oldItem = null;

		// Only do this if an item is there
		if (currentEquipment[slotIndex] != null)
		{
			// Add the item to the inventory
			oldItem = currentEquipment[slotIndex];

			//If there is a current item Switch
			if(switchItem == true)
				inventory.Add(oldItem);

            SetBlendShapeWeight(oldItem, 0);
            // Destroy the mesh
            if (currentMeshes[slotIndex] != null)
            {
                Destroy(currentMeshes[slotIndex].gameObject);
            }

			// Remove the item from the equipment array
			currentEquipment[slotIndex] = null;

			// Equipment has been removed so we trigger the callback
			if (onEquipmentChanged != null)
			{
				onEquipmentChanged.Invoke(null, oldItem);
			}
		}
        return oldItem;
	}

	// Unequip all items
	public void UnequipAll ()
	{
		for (int i = 0; i < currentEquipment.Length; i++)
		{
			Unequip(i, false);			
		}

        //EquipDefaults();
	}

	public void DropItem(Item dropI)
	{
		var it = dropI as Equipment;
		///WOrkig on 
		GameObject newItemDrop = Instantiate(itemDrop,player.transform.position + player.transform.up + player.transform.forward, Quaternion.identity);
		newItemDrop.name = it.name;
		newItemDrop.GetComponent<ItemPickup>().item = dropI;
		newItemDrop.transform.Find("ItemPreview").GetComponent<MeshFilter>().mesh = it.mesh.sharedMesh;
		newItemDrop.transform.Find("ItemPreview").GetComponent<MeshRenderer>().materials = it.mesh.sharedMaterials;

		//Change Item Size//
		newItemDrop.transform.Find("ItemPreview").GetComponent<Transform>().localScale = it.mesh.GetComponent<Transform>().localScale;
		//Debug.Log(dropI.Name);
		//Debug.Log(it.damageModifier);
		
		
		//newItemDrop.GetComponent<ItemDrop>().itemMesh = currentMeshes[i].sharedMesh;
	}

    void AttachToMesh(Equipment item, int slotIndex)
	{
        SkinnedMeshRenderer newMesh = Instantiate(item.mesh) as SkinnedMeshRenderer;
		//Mesh newMesh = Instantiate(item.mesh) as Mesh;
        newMesh.transform.parent = targetMesh.transform.parent;

        newMesh.rootBone = targetMesh.rootBone;
		newMesh.bones = targetMesh.bones;
		
		currentMeshes[slotIndex] = newMesh;


        SetBlendShapeWeight(item, 100);
       
	}

    void SetBlendShapeWeight(Equipment item, int weight)
    {
		foreach (MeshBlendShape blendshape in item.coveredMeshRegions)
		{
			int shapeIndex = (int)blendshape;
            targetMesh.SetBlendShapeWeight(shapeIndex, weight);
		}
    }

    void EquipDefaults()
    {
		foreach (Equipment e in defaultEquipment)
		{
			Equip(e);
		}
    }

	void Update ()
	{
		// Unequip all items if we press U
		if (Input.GetKeyDown(KeyCode.U))
			UnequipAll();
	}

	*/
}
