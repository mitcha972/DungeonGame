using UnityEngine;

public enum ItemType { Head, Chest, Legs, Sword, Bow, Unarmed, Shield, Potion}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject 
{
	new public string name = "New Item";	
	new public string description = "Item Description";
	public ItemType itemType;

	public Sprite icon = null;				
	
	[Header("InGame")]
	public int price;  

	// Called when the Item is pressed in the inventory
	public virtual void Use(GameObject owner, int SlotIndex)//use()
	{
		//Does Somthing...
	}
}
