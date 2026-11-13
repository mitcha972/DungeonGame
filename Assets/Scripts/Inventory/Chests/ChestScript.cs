using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : Interactable
{
    public ChestInventoryUI chestInventoryUI;
    public ChestInventory inventory;	// Our current inventory
    [SerializeField] private Loot[] itemDrops;

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
        if (interactOwner == null || inventory == null) return;
        var player = interactOwner.GetComponent<Player>();
        if (player == null || player.uiCanvasController == null || player.uiCanvasController.chestInventoryUI == null) return;
        var canvas = player.uiCanvasController;
        if (canvas.inv == null || !canvas.inv.activeSelf) canvas.ToogleInvetoryUI();

        //Set cehct ui to active
        //toogel inventory


        chestInventoryUI = canvas.chestInventoryUI;
        chestInventoryUI.gameObject.SetActive(true);
        chestInventoryUI.currentChest = this;
        chestInventoryUI.inventory = inventory;

        //Set items to chest items //FIX
        for(int i = 0; i < inventory.items.Count && i < chestInventoryUI.slots.Length; i++)
        {
            Debug.Log(inventory.items[i]);

            chestInventoryUI.slots[i].AddItem(inventory.items[i]);
        }
	}

}

/*
[System.Serializable]
public class Loot 
{
	public GameObject lootItem;
	public int dropChance;

	public Loot(GameObject LI, int DC)
    {
        lootItem = LI;
		dropChance = DC;
    }
}
*/

