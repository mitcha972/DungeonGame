using UnityEngine.EventSystems;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour 
{
	Camera cam;			// Reference to our camer
	PlayerCombat playerCombat; 
	PlayerStats myStats; 
	public bool lockedOn;
	public Interactable focus;	// Our current focus: Item, Enemy etc.
    private ThirdPersonController movementController;
	//Look at object
	Ray lookRay;
	RaycastHit lookHit;
	Interactable interactable,Tinteractable;
	[SerializeField] private TMP_Text interactText;

	[Header("Movement")]
     private Vector2 m_Input;
    // private Vector3 m_MoveDir = Vector3.zero;
    private Rigidbody m_Rigidbody;
    public bool canMove;
    private bool canRotate;
    private bool canAttack;
    
    [Header("Camera")]
    public bool mouseLocked;

    [Header("Animation")]
    private Animator _animator;
	CharacterAnimator animator;// = GetComponent<CharacterAnimator>();
    private bool _hasAnimator;
    private float _animationBlend;

    //Attack
    bool isHeavyAttack;
    float buttonDownTime;
    float heavyAttackThreshold = 0.25f;

	void Awake() 
	{
		cam = Camera.main;
		playerCombat = GetComponent<PlayerCombat>();
		myStats = GetComponent<PlayerStats>();
		animator = GetComponent<CharacterAnimator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        movementController = GetComponent<ThirdPersonController>();

        animator.EnableMovement += EnableMovement;
        animator.DisableMovement += DisableMovement;
	}	

	void Start() 
	{
		//lockedOn = false;
		focus = null;
		CurrentMouseMode = MouseMode.Locked;
        // Initialize animation related variables
        _hasAnimator = TryGetComponent(out _animator);

        canMove = true;
        canRotate = true;
	}

	void Update () 
	{
		MouseInput();
        GetInput();
        
        //Ability
        if (Input.GetKeyDown(KeyCode.T))
        {
            playerCombat.Ability();
        }
	}

	void FixedUpdate() 
	{		
		//Look at object
		lookRay = cam.ScreenPointToRay(Input.mousePosition);

		CheckFocus();
        //Checkinterractable();
	}

	void MouseInput()
	{
        //Set up a bool can input check
        // 

		// Handle right-click
		if (Input.GetMouseButtonDown(0))
			RightClick();

        LeftClick();		
	}

	void RightClick()
	{
		// Create a ray from the camera to the mouse position
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		// Cast the ray and check if it hits an interactable object
		if (Physics.Raycast(ray, out hit, 20))
		{
			Interactable interactable = hit.collider.GetComponent<Interactable>();
			if (interactable != null)
			{
				// Set the focus to the interactable object
				SetFocus(interactable);
			}
			else
				RemoveFocus();
		}
		else
			RemoveFocus();
	}

	void LeftClick()
	{
		// Handle left-click           
        if (Input.GetMouseButtonDown(1))
        {
            // store the time when the button was first pressed
            buttonDownTime = Time.time;
            // reset the heavy attack flag
            isHeavyAttack = false;
        }

        if (Input.GetMouseButton(1))
        {
            // calculate how long the button has been held down
            float holdTime = Time.time - buttonDownTime;

            // if the hold time is greater than the threshold, set the heavy attack flag
            if (holdTime >= heavyAttackThreshold && !isHeavyAttack)
            {
                isHeavyAttack = true;
                // Perform the heavy attack immediately
                if (playerCombat != null)
                {
                    Debug.Log("HeavyATtakc - 1");
                    playerCombat.Attack(true);
                    buttonDownTime = 0;
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            // check if it's a regular attack
            if (!isHeavyAttack && playerCombat != null)
            {
                playerCombat.Attack(false);
            }
            // reset the heavy attack flag
            isHeavyAttack = false;
        }
	}

	[SerializeField] MouseMode CurrentMouseMode;
    enum MouseMode{Locked,Unlocked}
    
    void ChangeMouseMode(MouseMode NewMouseMode)
    {
        if(CurrentMouseMode == NewMouseMode)
            return;

        CurrentMouseMode = NewMouseMode;

        switch (NewMouseMode)
        {
            case MouseMode.Locked:
                MouseLocked();
                break;

            case MouseMode.Unlocked:
                MouseUnlocked();
                break;
        }
    }

    private void MouseLocked()
    {
		//UnlockLock player rotation
		//

		//lock mouse (hid - gameplay)
		//

    }

    private void MouseUnlocked()
    {
        //Lock player rotation
		//

		//unlock lock mouse (Gui of clicking screen)
		//

		//Lock player right and left motion clicks
		// 
    }

	void UpdateText()
	{
		//c
	} 

	void RemoveText()
	{
		interactText.SetText("");
		interactable = null;
	} 
	
	void CheckFocus()
	{
		// Rotate towards the focused object if there is one
		if (lockedOn == false)
			return;

		//Debug.Log("SHoulf turn" + lockedOn);
		TurnTo();	
	}

    void Checkinterractable()
    {
        /*
			if (Physics.Raycast(lookRay, out lookHit, 20))
			{
				//Check if Obejct has Des
				Tinteractable = lookHit.collider.GetComponent<Interactable>();

				if ((Tinteractable == null && interactable == Tinteractable) || (Tinteractable != null && interactable == Tinteractable))
				{
					return;
				}

				//Change to no interact
				if (Tinteractable == null && interactable != Tinteractable)
				{
					RemoveText();
				}
				
				if (Tinteractable != null && interactable != Tinteractable)
				{
					//UpdateText();
					interactable = Tinteractable;
					
					//Look At Tip
					if(lookHit.collider.GetComponent<ObjectGrabbable>())
						interactText.SetText("[ " + interactable.gameObject.name + " ] \n Q - Pick Up");
				}
			}
		*/
    }

	void TurnTo()
	{
		// Calculate the direction towards the focused object
		Vector3 targetDirection = focus.transform.position - transform.position;
		targetDirection.y = 0; // Ignore vertical angle
		float singleStep = 2f * Time.deltaTime;
		Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
		// Rotate the player towards the focused object
		transform.rotation = Quaternion.LookRotation(newDirection);
	}

	// Set our focus to a new focus
	void SetFocus (Interactable newFocus)
	{
		//if click on already focused then defocus.
		if (newFocus == focus)
		{
			Debug.Log("???RemovedLocked On!!!!");
			RemoveFocus();
			return;
		}

		// If our focus has changed
		if (newFocus != focus)
		{
			// Defocus the old one
			if (focus != null)
				focus.OnDefocused();

			focus = newFocus;	// Set our new focus
		}
		newFocus.OnFocused(transform);
		newFocus.Interact(transform);

		//Check if new focus is an enemy
		if(focus.transform.tag == "Enemy")
		{		
			//IF so then run the lock on script
			lockedOn = true;
			mouseLocked = true;
			Debug.Log("Locked On!!!!");

			//Change mouse Lock on
			// 
		}
	}

	// Remove our current focus
	void RemoveFocus()
	{
		if (focus == null)
			return;

		focus.OnDefocused();
		focus = null;
		lockedOn = false;
		mouseLocked = false;
		//motor.StopFollowingTarget();
	}
  
    public void EnableMovement()
    {
        //lock or unlock player movement
        movementController.canMove = true;
        movementController.canRotate = true;
    }

    public void DisableMovement()
    {
        // Lock or unlock player rotation
        movementController.canMove = false;
        movementController.canRotate = false;
    }

    private void GetInput()//out float speed
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Store input as a vector.
        m_Input = new Vector2(horizontal, vertical);

         // Normalize input if it exceeds 1 in combined length.
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        if (Input.GetKey(KeyCode.LeftShift) && m_Input.magnitude > 0f)
        {
            // Find the dodge direction based on the WASD inputs
            int dodgeDirection = 0;

            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
            {
                if (horizontal > 0)
                {
                    dodgeDirection = 2; // Right
                }
                else
                {
                    dodgeDirection = 3; // Left
                }
            }
            else
            {
                if (vertical > 0)
                {
                    dodgeDirection = 1; // Front
                }
                else if (vertical < 0)
                {
                    dodgeDirection = 4; // Back
                }
            }

            // Perform the dodge action in the dodge direction
            if(dodgeDirection != 0)
                DoDodge(dodgeDirection);
        }

        // Update animation blend.
        // _animationBlend = Mathf.Lerp(_animationBlend,  speed * m_Input.sqrMagnitude, Time.deltaTime * 10f);
        // if (_animationBlend < 0.01f) _animationBlend = 0f;

        //_animator.SetFloat("Horizontal", m_Input.x);
        //_animator.SetFloat("Vertical", m_Input.y);
    }

    private void DoDodge(int direction)
    {
        //Check if Animation is Busy
        if (animator.AnimationIsBusy())
            return;
        
        animator.PlayTargetAnimation("Roll", true);
        // switch (direction)
        // {
        //      case 1: // Front
        //         animator.PlayTargetAnimation("Roll", true);
        //         break;
        //     case 2: // Right
        //         animator.PlayTargetAnimation("DodgeRight", true);
        //         break;
        //     case 3: // Left
        //         animator.PlayTargetAnimation("DodgeLeft", true);
        //         break;
        //     case 4: // Back
        //         animator.PlayTargetAnimation("RollBackwards", true);
        //         break;
        // }
        
        StartCoroutine(CheckDodge());
    }

    private IEnumerator CheckDodge()
    {
        // ...

        yield return new WaitForSeconds(.1f);

        _animator.SetBool("Dodge", false);
    }
}