using UnityEngine;

public enum ItemType { Head, Chest, Legs, Sword, Bow, Unarmed, Shield, Potion, Resource}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
	new public string name = "New Item";
	new public string description = "Item Description";
	public ItemType itemType;

	public Sprite icon = null;

	[Header("InGame")]
    [Tooltip("Optional world pickup prefab used when this item is dropped. Its ItemInteract is assigned this definition at runtime.")]
    public GameObject worldPrefab;
	public int price;
    [Tooltip("Category rules cannot buy this item; explicit merchant allowlist required.")]
    public bool tradeRestricted;

	// Called when the Item is pressed in the inventory
	public virtual void Use(GameObject owner, int SlotIndex)//use()
	{
		//Does Somthing...
	}
}
