using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScript : Interactable
{
    public ShopInventoryUI shopInventoryUI;
    public ShopInventory inventory;	// Our current inventory
    [SerializeField] private ShopLoot[] itemDrops;
    [Tooltip("Optional conversation before trading. Existing shops retain direct interaction.")]
    public DungeonGame.Dialogue.DialogueNpc dialogue;

    void Start()
    {
        if (inventory == null || itemDrops == null) return;
        foreach (var drop in itemDrops)
        {
            if (drop == null || drop.lootItem == null || Random.Range(0, 100) >= Mathf.Clamp(drop.dropChance, 0, 100)) continue;
            var pickup = drop.lootItem.GetComponent<ItemInteract>();
            if (pickup != null) inventory.Add(pickup.item);
        }
    }

    // When the player interacts with the item
	public override void Interact(Transform interactOwner)
	{
		if (dialogue != null) { dialogue.Interact(interactOwner); return; }
        TryOpenShop(interactOwner);
	}

    public bool TryOpenShop(Transform owner)
    {
        var state = owner != null ? owner.GetComponent<PlayerControlState>() : null;
        var ui = shopInventoryUI != null ? shopInventoryUI : state != null ? state.ShopUI : null;
        if (inventory == null || ui == null) return false;
        ui.OpenShop(this, state);
        return true;
    }
}

//Replace it Root class Script
[System.Serializable]
public class ShopLoot 
{
	public GameObject lootItem;
	public int dropChance;

	public ShopLoot(GameObject LI, int DC)
    {
        lootItem = LI;
		dropChance = DC;
    }
}
