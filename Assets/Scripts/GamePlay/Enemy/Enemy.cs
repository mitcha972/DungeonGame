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

	[Header("Navigation and Awareness")]
	[Min(0.1f)] public float repathInterval = 0.4f;
	[Min(0.05f)] public float destinationThreshold = 0.5f;
	[Min(0f)] public float patrolRadius = 8f;
	[Min(0f)] public float patrolWait = 2f;
	[Min(0f)] public float memoryDuration = 5f;
	[Range(1f, 360f)] public float fieldOfView = 120f;
	[Min(0f)] public float turnSpeed = 360f;
	[Min(0.1f)] public float acceleration = 12f;
	[Min(0.1f)] public float navMeshSampleRadius = 1.5f;
	[Min(0.1f)] public float stuckTimeout = 3f;
	[Min(0f)] public float attackRangeHysteresis = 0.25f;
	[Range(1f, 180f)] public float attackFacingAngle = 35f;
	[Header("Death")]
	[Tooltip("Zero keeps the corpse indefinitely; positive values remove it after this delay.")]
	[Min(0f)] public float corpseLifetime = 10f;
}

[System.Serializable]
public class DefaultEquipment
{
	public Equipment equipment;
	public int equipLevel;
	public int equipChance;
}
