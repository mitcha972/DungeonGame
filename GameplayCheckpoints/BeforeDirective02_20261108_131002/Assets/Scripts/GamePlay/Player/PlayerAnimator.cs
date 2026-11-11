using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Move all to Character animator
public class PlayerAnimator : CharacterAnimator  //Change to enemy animator
{    
    PlayerController controller;
    
    void Start()
	{
		controller = GetComponent<PlayerController>();//diffrent from enemy
	}

    // public override void DisableMovemnt()
    // {
    //     // TODO: Implement this method
    //     //movementController.canRotate = false;
    //     //controller.CanRotate(false);
    //     //controller.CanMove(false);
    // }

    // public override void EnableMovement()
    // {
    //     //TODO 
    //     //movementController.canMove = true;
    //     //Debug.Log("Movement is Enabled");
    //     //controller.CanRotate(true);
    //     //controller.CanMove(true);
    // }   
}