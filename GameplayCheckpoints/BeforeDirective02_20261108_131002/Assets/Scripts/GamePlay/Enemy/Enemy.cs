using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class Enemy : ScriptableObject 
{
	new public string name = "New Enemy";	// Name of the item
	
	[Header("Info")]
	//eumm race ()
	//public Materials[] Skins;
	public int XP;

	[Header("Base Stats")]
	public int health; //change to health
	//public int mana
	public int armor;		
	public int damage;
	public int attackRange; 
	public int sightRange;
	public int movementSpeed;
	public int attackSpeed;

	//Starting Items (wapons/armor)[]
	[Header("Items")]
	public DefaultEquipment[] defaultEquipment;
	public Loot[] itemDrops;		
}

[System.Serializable]
public class DefaultEquipment
{
	public Equipment equipment;
	public int equipLevel;
	public int equipChance;
}
