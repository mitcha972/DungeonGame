using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : CharacterAnimator
{
    EnemyAi controller;

    void Start()
    {
        controller = GetComponent<EnemyAi>(); //Diffrence from palyer
    }

    // public override void EnableMovement()
    // {
    //     controller.EnableMovement();
    // }

    // public override void DisableMovemnt()
    // {
    //     controller.DisableMovement();
    // }

    
}
