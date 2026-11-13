using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Loot
{
	public GameObject lootItem;
	public int dropChance;

	public Loot(GameObject LootItem, int DropChance)
    {
        lootItem = LootItem;
		dropChance = DropChance;
    }
}
