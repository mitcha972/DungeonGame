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
		animator = GetComponentInParent<CharacterAnimator>();
        if (attackHitBox == null)
        {
            var ownerCombat = GetComponentInParent<CharacterCombat>();
            if (ownerCombat != null && ownerCombat.attackHitBox != null) attackHitBox = ownerCombat.attackHitBox.gameObject;
        }
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
	    if (newItem == null || inventory == null || SlotIndex < 0 || SlotIndex >= inventory.items.Count || inventory.items[SlotIndex] != newItem) return;
	    currentEquipment.TryGetValue(newItem.equipSlot, out Equipment oldItem);
	    inventory.Swap(oldItem, SlotIndex);
	    ReplaceEquipment(newItem, oldItem);
	}

	public virtual void Equip(Equipment newItem)
	{
	    if (newItem == null) return;
	    currentEquipment.TryGetValue(newItem.equipSlot, out Equipment oldItem);
	    ReplaceEquipment(newItem, oldItem);
	}

	private void ReplaceEquipment(Equipment newItem, Equipment oldItem)
	{
	    EquipmentSlot slot = newItem.equipSlot;
	    if (oldItem != null) SetBlendShapeWeight(oldItem, 0);
	    if (currentMeshes.TryGetValue(slot, out SkinnedMeshRenderer oldMesh) && oldMesh != null) Destroy(oldMesh.gameObject);
	    currentMeshes.Remove(slot);
	    currentEquipment[slot] = newItem;
	    UpdateCurrentEquipmentArray();
	    if (newItem.mesh != null && targetMesh != null) AttachToMesh(newItem, slot);
	    onEquipmentChanged?.Invoke(newItem, oldItem);
	}

	public virtual Equipment Unequip(EquipmentSlot slotIndex, int SlotIndex, Equipment newItem)
	{
	    currentEquipment.TryGetValue(slotIndex, out Equipment oldItem);
	    if (inventory != null && SlotIndex >= 0 && SlotIndex < inventory.items.Count) inventory.Swap(oldItem, SlotIndex);
	    if (newItem != null) ReplaceEquipment(newItem, oldItem);
	    else Unequip(slotIndex);
	    return oldItem;
	}

	public virtual Equipment Unequip(EquipmentSlot slotIndex)
	{
	    if (!currentEquipment.TryGetValue(slotIndex, out Equipment oldItem)) return null;
	    currentEquipment.Remove(slotIndex);
	    UpdateCurrentEquipmentArray();
	    SetBlendShapeWeight(oldItem, 0);
	    if (currentMeshes.TryGetValue(slotIndex, out SkinnedMeshRenderer mesh) && mesh != null) Destroy(mesh.gameObject);
	    currentMeshes.Remove(slotIndex);
	    onEquipmentChanged?.Invoke(null, oldItem);
	    return oldItem;
	}

public void UnequipAll()
	{
		foreach (EquipmentSlot slot in currentEquipment.Keys.ToList())
		{
			Unequip(slot);
		}
	}

    public void UnequipAllToInventory()
    {
        if (inventory == null) return;
        foreach (EquipmentSlot slot in currentEquipment.Keys.ToList())
        {
            if (currentEquipment.TryGetValue(slot, out Equipment equipment) && equipment != null && inventory.Add(equipment))
                Unequip(slot);
        }
    }

    public void DropItem(Item dropI)
    {
        TryDropItem(dropI);
    }

    public bool TryDropItem(Item dropI)
    {
        var equipment = dropI as Equipment;
        if (equipment == null || equipment.mesh == null || itemDrop == null) return false;

        GameObject droppedItem = Instantiate(itemDrop, transform.position + transform.up + transform.forward, Quaternion.identity);
        var pickup = droppedItem.GetComponent<ItemInteract>();
        Transform preview = droppedItem.transform.Find("ItemPreview");
        var filter = preview != null ? preview.GetComponent<MeshFilter>() : null;
        var renderer = preview != null ? preview.GetComponent<MeshRenderer>() : null;
        if (pickup == null || filter == null || renderer == null)
        {
            droppedItem.SetActive(false);
            Destroy(droppedItem);
            Debug.LogWarning("Item drop requires ItemInteract and an ItemPreview with MeshFilter and MeshRenderer.", this);
            return false;
        }

        droppedItem.name = equipment.name;
        pickup.item = dropI;
        filter.sharedMesh = equipment.mesh.sharedMesh;
        renderer.sharedMaterials = equipment.mesh.sharedMaterials;
        preview.localScale = equipment.mesh.transform.localScale;
        return true;
    }

	public void AttachToMesh(Equipment item, EquipmentSlot slotIndex)
	{
		if (item == null || item.mesh == null || targetMesh == null) return;

	    SkinnedMeshRenderer newMesh = Instantiate(item.mesh) as SkinnedMeshRenderer;
		if (newMesh == null) return;
		newMesh.transform.SetParent(targetMesh.transform.parent, false);

		if(item.equipSlot == EquipmentSlot.MainHand)
		{
			BoxCollider newCollider = newMesh.GetComponent<BoxCollider>();
			if (newCollider != null && attackHitBox != null && attackHitBox.TryGetComponent<BoxCollider>(out var hitCollider))
			{
	            hitCollider.center = newCollider.center;
	            hitCollider.size = newCollider.size;
			}
		}

	    newMesh.rootBone = targetMesh.rootBone;
		newMesh.bones = targetMesh.bones;
	    if (slotIndex == EquipmentSlot.MainHand && attackHitBox != null && attackHitBox.TryGetComponent<AttackHitBox>(out var hitbox)) hitbox.ConfigureWeapon(newMesh);
		
		currentMeshes[slotIndex] = newMesh;

	    SetBlendShapeWeight(item, 100);
       
	}

	public void SetBlendShapeWeight(Equipment item, int weight)
	{
		if (item == null || targetMesh == null || targetMesh.sharedMesh == null || item.coveredMeshRegions == null) return;
		foreach (MeshBlendShape blendShape in item.coveredMeshRegions)
		{
			int shapeIndex = (int)blendShape;
			if (shapeIndex >= 0 && shapeIndex < targetMesh.sharedMesh.blendShapeCount)
                targetMesh.SetBlendShapeWeight(shapeIndex, weight);
		}
	}

	void Update ()
	{
        if (!Input.GetKeyDown(KeyCode.U)) return;
        var controlState = GetComponentInParent<PlayerControlState>();
        if (controlState != null && !controlState.GameplayAllowed) return;
        if (animator != null && animator.AnimationIsBusy()) return;
        UnequipAllToInventory();
	}
}
