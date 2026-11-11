using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Keeps track of enemy stats*/ //controller ?
public class EnemyStats : CharacterStats 
{
	[SerializeField] Enemy enemy;

	Loot[] itemDrops;
	EnemyAi enemyAi;
	
	void Awake()
	{		
		equipmentManager = GetComponent<EquipmentManager>();
		equipmentManager.onEquipmentChanged += OnEquipmentChanged;
		enemyAi = GetComponent<EnemyAi>();
		
		//Enemy stats
		maxHealth = enemy.health;
		damage = enemy.damage;
		armor = enemy.armor;
		range = enemy.attackRange;
		itemDrops = enemy.itemDrops;
		attackSpeed = enemy.attackSpeed;
		movementSpeed = enemy.movementSpeed;
		sightRange = enemy.sightRange;
	}

	void Start()
	{
		StartUP();
	}

	public override void OnEquipmentChanged (Equipment newItem, Equipment oldItem)
	{
		base.OnEquipmentChanged(newItem, oldItem);
	}

	public override void TakeDamage (int damageTaken, Transform attackOwner, float attackDirection)
	{
		base.TakeDamage (damageTaken, attackOwner,attackDirection);

		//Check if Eenmy Has A targt //If not Set this attacker at target
		if(enemyAi.target == null)
		{
			//set target
			enemyAi.target = attackOwner;

			//Change CombatState to incombat
			//
			//enemyAi.ChangeState(enemyAi.State.InCombat);
		}	
	}

	public override void Die(Transform attackOwner)
	{
		base.Die(attackOwner);

		//Give Player XP;
		attackOwner.gameObject.GetComponent<PlayerStats>().AddXP(enemy.XP);

		// DropItem Chance
		int rand = 0;

		for(int i = 0; i < itemDrops.Length; i++)
		{
			rand = Random.Range(0,100);

			if(rand <= itemDrops[i].dropChance)
			{
				GameObject newItemDrop = Instantiate(itemDrops[i].lootItem,transform.position + transform.up, Quaternion.identity);
				Rigidbody rb = newItemDrop.GetComponent<Rigidbody>();
				rb.AddForce(0, Random.Range(1,3), Random.Range(1,3), ForceMode.Impulse);
			}
		}	
		
		//Death Effect
		enemyAi.Die();
	}
}
/*
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
*/