using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Handles interaction with the Enemy */
[RequireComponent(typeof(CharacterStats))]
public class EnemyInteract : Interactable 
{
	PlayerManager playerManager;
	CharacterStats myStats;
	
	private Outline outline; 

	void Start ()
	{
		playerManager = PlayerManager.instance;
		myStats = GetComponent<CharacterStats>();
		outline = GetComponent<Outline>();
	}

	public override void OnFocused (Transform playerTransform)
	{
		base.OnFocused (playerTransform);
		outline.enabled = true;
	}

	public override void OnDefocused()
	{
		base.OnDefocused();
		outline.enabled = false;
	}
}
