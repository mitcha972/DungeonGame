using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScript : Interactable
{
    public ShopInventoryUI shopInventoryUI;
    public ShopInventory inventory;	// Our current inventory
    [SerializeField] private ShopLoot[] itemDrops;

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
        if (inventory == null || shopInventoryUI == null) return;

		Debug.Log("Shop");

        //Set cehct ui to active
        shopInventoryUI.gameObject.SetActive(true);
        shopInventoryUI.currentShop = this;
        shopInventoryUI.inventory = inventory;

        //Set items to chest items //FIX
        for(int i = 0; i < inventory.items.Count && i < shopInventoryUI.slots.Length; i++)
        {
            Debug.Log(inventory.items[i]);
            shopInventoryUI.slots[i].AddItem(inventory.items[i]);
            //shopInventoryUI.slots[shopInventoryUI.inventory.items.Count+i].AddItem(inventory.items[i]);
            
            //shopInventoryUI.inventory.Add(inventory.items[i]);
        }
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
