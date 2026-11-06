using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CharacterCombat 
{
	[SerializeField] private Transform aim;
	
	void Update()
	{
		CheckBlock(); //move to player controller
	}

	public override void EnableProjectile()
	{
		// Calculate the spawn position and initial direction of the projectile
		Vector3 spawnPosition = attackHitBox.transform.position;// + transform.up * GetComponent<Collider>().bounds.extents.y + transform.forward * 1f;
    	Vector3 aimDirection = aim.position - transform.position;
		
		// Rotate the aim transform to face the aim direction
		aim.rotation = Quaternion.LookRotation(aimDirection, Vector3.up);

		GameObject pro = Instantiate(currentProjectile, spawnPosition, aim.rotation);
		Projectile DHB = pro.GetComponent<Projectile>();
		Rigidbody rb = pro.GetComponent<Rigidbody>();

		DHB.myStats = this.myStats;
		DHB.owner = this.gameObject;

		rb.AddForce(aimDirection.normalized * DHB.speed, ForceMode.Impulse);
	}

	public override void EnableProjectile(Abilities ability, int damage)
	{
		// Calculate the spawn position and initial direction of the projectile
		Vector3 spawnPosition = attackHitBox.transform.position;//transform.position + transform.up * GetComponent<Collider>().bounds.extents.y + transform.forward * 1f;
    	Vector3 aimDirection = aim.position - transform.position;
		
		// Rotate the aim transform to face the aim direction
		aim.rotation = Quaternion.LookRotation(aimDirection, Vector3.up);

		GameObject pro = Instantiate(ability.projectile, spawnPosition, aim.rotation);
		Projectile DHB = pro.GetComponent<Projectile>();
		Rigidbody rb = pro.GetComponent<Rigidbody>();
		pro.tag = gameObject.tag;

		DHB.StartUp(this.myStats, this.gameObject, ability.damage, ability.range, ability.speed);

		rb.AddForce(aimDirection.normalized * ability.speed, ForceMode.Impulse);
	}

	void CheckBlock() //move to player controller 
	{
		if(canBlock == true)
		{
			if(Input.GetKey(KeyCode.Z) && isBlocking == false)
				BlockStart(); //combat.blcokStart
		}
		else
		{
			if(isBlocking == true)// diable block if isBlocking
				BlockDrop();////combat.blcokStart
		}

		//If remove z key and was blocking, drop block
		if(Input.GetKeyUp(KeyCode.Z) && isBlocking == true)
			BlockDrop();
	}	

}
