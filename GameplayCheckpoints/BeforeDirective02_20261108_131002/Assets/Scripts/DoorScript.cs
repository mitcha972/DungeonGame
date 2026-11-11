using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorScript : Interactable
{
    private UnityEngine.AI.NavMeshObstacle door;
    private Animator animator;
    public bool isOpen;

    void Awake()
    {
        door = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        animator = GetComponent<Animator>();
    }

    public override void Interact (Transform interactOwner)
	{        
        Door();
	}

    void Door()
    {
        isOpen = !isOpen;

        //Check if Door is Closed Or open
        if(isOpen) //close the door 
        {
            //Open Door Animation
            //animator.SetTrigger("OpenDoor");
            animator.Play("DoorOpen", 0, 0f);

            //Remove Contact
            //

            //NaveMesh 
            door.carving = isOpen;
        }
        else
        {
            //Close Door Animation
            animator.Play("DoorClose", 0, 0f);

            //Add Contact
            //

            //NaveMesh 
            door.carving = isOpen;
        }        
    }
}
