using UnityEngine;

/* Base class that player and enemies can derive from to include stats */
public class CharacterStats : MonoBehaviour 
{
	//equipmentManager
	public EquipmentManager equipmentManager; //Make private
	//public GameObject bloodSplat;

	// Stats
	[HideInInspector] public int maxHealth;
	public int currentHealth;
	public int damage;
	[HideInInspector] public int armor;
	[HideInInspector] public int range;
	[HideInInspector] public int attackSpeed;
	[HideInInspector] public int movementSpeed;
	[HideInInspector] public int sightRange;

    public event System.Action<int, int> OnHealthChanged;

	public void StartUP() //Fixxxxx
	{
		currentHealth = maxHealth;
	}

	// Called when an item gets equipped/unequipped
	public virtual void OnEquipmentChanged (Equipment newItem, Equipment oldItem)
	{
		// Add new modifiers
		if (newItem)
		{
			armor += newItem.armorModifier;
			damage += newItem.damageModifier;
			range += newItem.rangeModifier;
		}

		// Removes Old Modifiers
		if (oldItem)
		{
			armor -= oldItem.armorModifier;
			damage -= oldItem.damageModifier;
			range -= oldItem.rangeModifier;
		}	

		//Update Enemy equip
		EnemyAi enemyAi = this.gameObject.GetComponent<EnemyAi>();
		if(enemyAi)
			enemyAi.UpdateEquipment();
			//enemyAi.attackRange = range;	
	}

	// Damage The Character // Called from character combat
	public virtual void TakeDamage (int damageTaken, Transform attackOwner, float attackDirection)
	{
		//Play Damage Animation
		this.gameObject.GetComponent<CharacterAnimator>().GetHit(attackDirection);
		/*
			//Bleed
			//GameObject bloodSplater = Instantiate(bloodSplat, attackDirection, Quaternion.identity);
			
			
			if(bloodSplat != null)
			{
				//Find direction
				//attackDirection - this.transform;
				GameObject bloodSplat = Instantiate(bloodSplat, attackDirection, Quaternion.identity);
			}
		*/

		// Subtract Damage Based on Armomr FIXXXX***
		damageTaken -= armor;
		damageTaken = Mathf.Clamp(damageTaken, 1, int.MaxValue);

		// Damage The Character
		currentHealth -= damageTaken;
		Debug.Log(transform.name + ": Takes - " + damageTaken + " Damage");

		//Update Health
        if (OnHealthChanged != null)
        {
           OnHealthChanged(maxHealth, currentHealth);
        }

		// If Health Reaches Zero Then Die
		if (currentHealth <= 0)
		{
			Die(attackOwner);
		}
	}

	public virtual void TakeDamage (int damageTaken)
	{
		damageTaken -= armor;
		damageTaken = Mathf.Clamp(damageTaken, 1, int.MaxValue);

		// Damage The Character
		currentHealth -= damageTaken;
		Debug.Log(transform.name + ": Takes - " + damageTaken + " Damage");

		//Update Health
        if (OnHealthChanged != null)
        {
           OnHealthChanged(maxHealth, currentHealth);
        }

		// If Health Reaches Zero
		if (currentHealth <= 0)
		{
			Die(null);
		}
	}

	public virtual void Die(Transform attackOwner)
	{
		// Die in some way
		//this.gameObject.GetComponent<CharacterAnimator>().Death();
		Debug.Log(transform.name + ": Was Killed By - " + attackOwner.name);
	}
}

