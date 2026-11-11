using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour 
{	
	//Run OnAttack action throuch multiple scripts()
	public event System.Action<bool> OnAttack;
	
	public CharacterStats myStats;
    CharacterStats opponentStats;

	public GameObject currentProjectile;
	public GameObject bloodSplat;
	public AttackHitBox attackHitBox;
	public Abilities abilities;

	public bool InCombat;

	//Attacking 
	private float attackSpeed;
    const float combatCooldown = 5;//fix
    float lastAttackTime;
	public bool canAttack;
	public bool attackIsReady;//fix

	//Blocking
	public BlockHitBox blockHitBox;
	public bool isBlocking;
	public bool canBlock;//fix
	public float cooldownTime; 

	CharacterAnimator animator;
	Rigidbody rb;
	
	private void Awake()
	{
		myStats = GetComponent<CharacterStats>();
		animator = GetComponent<CharacterAnimator>();
		rb = GetComponent<Rigidbody>();

		Transform blockChild = transform.Find("Block");

		if (blockChild != null)
			blockHitBox = blockChild.GetComponent<BlockHitBox>();
	 
	 	attackSpeed = myStats.attackSpeed;
	}

	private void Start()
	{
		//Set Bools
		canBlock = true;
		isBlocking = false;
		canAttack = true;
		attackIsReady = true;

		cooldownTime  = 0f;		
	}

	public void Ability()
	{
		//Check if Animation is Busy
		if (animator.AnimationIsBusy())
			return;

		//Play Abiltiy Animation
		animator.PlayAbilityAnimation(abilities.startAnimation);
		
		//Play the ability particl effect
		//		

		//If has Projectile
		if(abilities.projectile) EnableProjectile(abilities,abilities.damage);
	}

	public void Attack(bool isHeavyAttack)
	{
		//Check if Player Animation is Busy
		if (animator.AnimationIsBusy() || !attackIsReady)
			return;

		// //Attack Function
		// if ()
        //  	return;

		//If there is an onattack functuion run it (Used by: Cahracter Animator)
		if (OnAttack != null)
			OnAttack(isHeavyAttack);

		attackIsReady = false;

		StartCoroutine(AttackCooldown());
		
		InCombat = true;
		lastAttackTime = Time.time;
	}

	private IEnumerator AttackCooldown()
	{		
		//Find current attack animation length
		//attackAnimationTime = 

		cooldownTime = 1f;
		//cooldownTime = (2f/attackSpeed) + attackAnimationTime;

		while (cooldownTime > 0f)
    	{
			cooldownTime -= Time.deltaTime;
			yield return null;
    	}

		//When cooldown loop is done
		attackIsReady = true;
	}

	public virtual void TakeDamage (int damageTaken, Transform attackOwner, float attackDirection, Vector3 attackPos, bool isBlocked)
	{
		myStats.TakeDamage(damageTaken, attackOwner, attackDirection);

		//Debug.Log("hit from the " + attackDirection);

		//if is Blcoked
		if(isBlocked)
		{
			//Play Blocked Animation
			animator.PlayTargetAnimation("BlockImpact", true);
		}
		else
		{			
			//Play Damage Animation
			animator.GetHit(attackDirection);
			
			// Calculate the knockback direction opposite to the attack direction
			Vector3 knockbackDirection = (transform.position - attackOwner.position).normalized;
			knockbackDirection.y = 0f;
			float knockbackDistance = 2f;// * knockbackDuration;

			// Calculate the target position to move to
			Vector3 targetPosition = transform.position + knockbackDirection* knockbackDistance;// knockbackDirection * knockbackDistance;
			StartCoroutine(KnockbackForce(targetPosition));

			//Bleed
			if(bloodSplat != null)
			{
				// Instantiate blood splatter
				GameObject bloodSplatter = Instantiate(bloodSplat, attackPos, Quaternion.Euler(new Vector3(0, 0, 0)));

				// Set the parent of the blood splatter to this object's parent
				bloodSplatter.transform.parent = transform.parent;
			}
		}
	}

	private IEnumerator KnockbackForce(Vector3 targetPosition)
	{
		float timeElapsed = 0f;
		Vector3 startingPosition = transform.position;

		while (timeElapsed < 0.2f)
		{
			// Calculate the interpolation factor based on the elapsed time and duration
			float t = timeElapsed / 0.2f;

			// Move the character gradually towards the target position
			transform.position = Vector3.Lerp(startingPosition, targetPosition, t);

			timeElapsed += Time.deltaTime;

			yield return null;
		}
	}

	public void BlockStart()
	{
		if (animator.AnimationIsBusy())
			return;

		if(isBlocking)
			return;
		
		//animator.SetBool("Blocking",isBlocking );
		animator.PlayTargetAnimation("BlockStart", true);
		Debug.Log("Block Statupppp");
		blockHitBox.EnableBlock();
		isBlocking = true;
	}
	
	public void BlockDrop()
	{
		if(isBlocking == false)
			return;

		animator.PlayTargetAnimation("BlockDrop", true);
		Debug.Log("DropBlock KKKK ");
		blockHitBox.DisableBlock();
		isBlocking = false;
	}

	void CheckInComabt()
	{
		if (Time.time - lastAttackTime > combatCooldown)
        {
            InCombat = false;
        }
	}
	
	public virtual void EnableProjectile()
    {
		//...Does Something
	}

	public virtual void EnableProjectile(Abilities ability, int damage)
    {
		//...Does Something
	}

	public void EnableDamage()
	{
		//Debug.Log("Damage Enabled");
		attackHitBox.EnableDamage();
	}

	public void DisableDamage()
	{
		//Debug.Log("Damage Disabled");
		attackHitBox.DisableDamage();
	}

}
