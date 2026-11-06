using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : CharacterCombat 
{
	public override void EnableProjectile() //unarmed
    {
		
		GameObject pro = Instantiate(currentProjectile, transform.position + transform.up*1.5f, Quaternion.identity);
		Projectile DHB =  pro.GetComponent<Projectile>();
		Rigidbody rb =  pro.GetComponent<Rigidbody>();
		
		DHB.myStats = this.myStats;//FIX
		DHB.owner = this.gameObject;

		pro.tag = gameObject.tag; // Set the tag of the created object to the tag of the current GameObject
		
		Transform Tr = GetComponent<EnemyAi>().target;// Targte object
		
		if(Tr) pro.transform.LookAt(Tr.position + Tr.up);  

		//Add A aim calculation
		// 
		
		rb.AddForce(pro.transform.forward * DHB.speed, ForceMode.Impulse); //DHB.weaponClass.speed
		
    }

	public override void EnableProjectile(Abilities ability, int damage) //unarmed
    {
		
		GameObject pro = Instantiate(ability.projectile, transform.position + transform.up*1.5f, Quaternion.identity);
		Projectile DHB =  pro.GetComponent<Projectile>();
		Rigidbody rb =  pro.GetComponent<Rigidbody>();
		pro.tag = gameObject.tag; // Set the tag of the created object to the tag of the current GameObject

		DHB.StartUp(this.myStats, this.gameObject, ability.damage, ability.range, ability.speed);

		Transform Tr = GetComponent<EnemyAi>().target;// Targte object
		if(Tr) pro.transform.LookAt(Tr.position + Tr.up);  
		
		rb.AddForce(pro.transform.forward * ability.speed, ForceMode.Impulse); //DHB.weaponClass.speed
		
    }

}
