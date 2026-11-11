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

	public bool IsDead { get; private set; }

	public virtual void TakeDamage(int damageTaken, Transform attackOwner, float attackDirection)
	{
	    if (IsDead || currentHealth <= 0 || damageTaken <= 0) return;
	    int resolvedDamage = Mathf.Max(1, damageTaken - Mathf.Max(0, armor));
	    currentHealth = Mathf.Max(0, currentHealth - resolvedDamage);
	    OnHealthChanged?.Invoke(maxHealth, currentHealth);
	    if (currentHealth == 0)
	    {
	        IsDead = true;
	        Die(attackOwner);
	    }
	}

	public virtual void TakeDamage(int damageTaken)
	{
	    TakeDamage(damageTaken, null, 0f);
	}

	public virtual void Die(Transform attackOwner)
	{
	    IsDead = true;
	    Debug.Log(transform.name + " killed by " + (attackOwner != null ? attackOwner.name : "environment"), this);
	}
}

