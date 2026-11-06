using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonExit : Interactable
{
    // Update is called once per frame
   // When the player interacts with the item
	public override void Interact(Transform interactOwner)
	{
		transform.parent.parent.GetComponent<DungeonGenerator>().ReStart();
		GameController.floor += 1;
        Debug.Log("Next Floor");
	}
}
