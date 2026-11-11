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

    void Door() => SetOpen(!isOpen);

    public void SetOpen(bool open, bool immediate = false)
    {
        isOpen = open;
        if (animator == null) animator = GetComponent<Animator>();
        if (door == null) door = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (animator != null)
        {
            animator.Play(open ? "DoorOpen" : "DoorClose", 0, immediate ? 1f : 0f);
            if (immediate) animator.Update(0f);
        }
        if (door != null)
        {
            door.enabled = !open;
            door.carving = !open;
        }
    }
}
