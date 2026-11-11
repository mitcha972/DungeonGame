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
        //Add Items to iventory
        int rand = 0;
       
        for(int i = 0; i < itemDrops.Length; i++)
        { 
            rand = Random.Range(0,100);
            //int tt = GameController.difficulty + GameController.floor;
            // ttt + rand 
            if(rand <= itemDrops[i].dropChance)
			{
                inventory.Add(itemDrops[i].lootItem.GetComponent<ItemInteract>().item);
            }
        }
    }

    // When the player interacts with the item
	public override void Interact(Transform interactOwner)
	{
		base.Interact(interactOwner);

        //Set cehct ui to active
        //toogel inventory
        interactOwner.GetComponent<Player>().uiCanvasController.ToogleInvetoryUI();

        chestInventoryUI = interactOwner.GetComponent<Player>().uiCanvasController.chestInventoryUI;
        chestInventoryUI.gameObject.SetActive(true);
        chestInventoryUI.currentChest = this;
        chestInventoryUI.inventory = inventory;

        //Set items to chest items //FIX
        for(int i = 0; i < inventory.items.Count; i++)
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

