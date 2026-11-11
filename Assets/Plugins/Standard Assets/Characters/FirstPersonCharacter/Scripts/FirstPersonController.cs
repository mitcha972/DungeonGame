using System;
using System.Collections;
using UnityEngine;
//using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

//#pragma warning disable 618, 649
//[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
public class FirstPersonController : MonoBehaviour
{
    // Movement-related fields
    [Header("Movement")]
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed, m_JumpSpeed, m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_StickToGroundForce, m_GravityMultiplier;
    [SerializeField] private float m_StepInterval;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    public bool canMove;
    private bool canRotate;
    
    // Camera-related fields
    [Header("Camera")]
    public MouseLook m_MouseLook;
    public bool mouseLocked;
    private Camera m_Camera;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    private Vector3 m_OriginalCameraPosition;

    // Audio-related fields
    [Header("Audio")]
    [SerializeField] private AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from
    [SerializeField] private AudioClip m_JumpSound; // the sound played when character leaves the ground
    [SerializeField] private AudioClip m_LandSound; // the sound played when character touches back on ground
    private AudioSource m_AudioSource;

    // Animation-related fields
    [Header("Animation")]
    private Animator _animator;
    private bool _hasAnimator;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private float _animationBlend;
    
    // Gravity/Jump-related fields
    [Header("Gravity/Jump-related fields")]
    public float m_gravity;
    //public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;
    public float jumpHeight;
    private bool m_Jump;
    private bool m_Jumping;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // Timeout-related fields
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private Rigidbody m_Rigidbody;

    public bool m_grounded;
    private RaycastHit m_GroundHit;

    private void Start()
    {
        // Initialize components
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_AudioSource = GetComponent<AudioSource>();

        // Set up camera effects
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);

        // Initialize animation related variables
        _hasAnimator = TryGetComponent(out _animator);
        AssignAnimationIDs();
        //characterAnimator = GetComponent<CharacterAnimator>();

        // Initialize jumping and falling timeout
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        // Initialize jump input
        //m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");

        // Initialize step sounds
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;

        // Initialize jumping state
        m_Jumping = false;

        // Initialize mouse look
        m_MouseLook.Init(transform, m_Camera.transform);

        canMove = true;
        canRotate = true;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

 /*
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(m_Rigidbody.transform.position + Vector3.up * 0.1f,0.25f);
        // Perform the spherecast
        RaycastHit hit;
        if (Physics.SphereCast(m_Rigidbody.transform.position + Vector3.up * 0.1f, 0.25f, Vector3.down, out hit, 1, ~GroundLayers))
        {
            // Draw a green sphere to show the spherecast hit point
            Gizmos.color = Color.green;
        }
        else
        {
            // Draw a red sphere to show the maximum range of the spherecast
            Gizmos.color = Color.red;
        }
        Gizmos.DrawSphere(m_Rigidbody.transform.position + Vector3.up * 0.1f,0.25f);
        
    }
 */

    private void FixedUpdate()
    {
        // Rotate the camera view based on mouse input
        RotateView();

        // Check if the player is currently grounded
        GroundedCheck();

        // Move the player based on input
        Move();

        JumpAndGravity();
    }

    public void CanMove(bool _canMove)
    {
        //lock or unlock player movement
        canMove = _canMove;
    }

    public void CanRotate(bool _canRotate)
    {
        // Lock or unlock player rotation
        canRotate = _canRotate;
    }

    private void GroundedCheck()
    {
        // Check if the player is on the ground using a spherecast
        RaycastHit hit;
        if (Physics.SphereCast(m_Rigidbody.transform.position + Vector3.up * 0.1f, 0.25f, Vector3.down, out hit, 1, ~GroundLayers))
        {       
            // Player is grounded
            m_grounded = true;
            //m_GroundHit = hit;
        }
        else
        {
            // Player is not grounded
            m_grounded = false;
            //m_GroundHit = new RaycastHit();
        }

        // Update grounded animation bool
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, m_grounded);
        }
    }

    private void Move()
    {
        if (canMove == false)
            return;

        // Get the input for the speed
        float speed;
        GetInput(out speed);

        // Always move the camera forward 
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // Project the desired move vector onto the surface normal to get the desired move position
        Vector3 moveDir = Vector3.ProjectOnPlane(desiredMove, m_GroundHit.normal).normalized;

        // Set the move direction based on the desired move and speed
        moveDir *= speed;
    
        //Project the desired move vector onto the surface normal to get the desired move position
        desiredMove = Vector3.ProjectOnPlane(desiredMove, m_MoveDir).normalized;
        
        // Move the character using Rigidbody
        m_Rigidbody.MovePosition(m_Rigidbody.position + moveDir * Time.fixedDeltaTime);

        // Update the step cycle
        ProgressStepCycle(speed);

        // Update the camera position
        UpdateCameraPosition(speed);

        // Update the animator
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
        }
    }

    private void RotateView()
    {
        //rotates the mouse
        if(canRotate == false)
            return;

        if(mouseLocked == false)
            m_MouseLook.LookRotation(transform, m_Camera.transform);
    }

    private void JumpAndGravity()
    {
        //m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");

        // Check for jump input and if player is grounded
        if (Input.GetKeyDown(KeyCode.Space) && m_grounded)
        {
            // Apply jump velocity
            m_Rigidbody.linearVelocity = new Vector3(m_Rigidbody.linearVelocity.x, jumpHeight, m_Rigidbody.linearVelocity.z);

            // Set jump animation bool to true
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
            }
        }

        // Check if player has landed, and turn off jump animation bool
        if (_hasAnimator && _animator.GetBool(_animIDJump) && m_Rigidbody.linearVelocity.y <= 0)
        {
            _animator.SetBool(_animIDJump, false);
        }
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    private void ProgressStepCycle(float speed)
    {
        //FIXXX
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                            Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        //PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }

        // pick & play a random footstep sound from the array, excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);

        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void UpdateCameraPosition(float speed)
    {
        // Only update camera position if head bob is enabled
        if (!m_UseHeadBob)
        {
            return;
        }

        Vector3 newCameraPosition;
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            // Do head bob animation when player is walking or running on ground
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                    (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y;
        }
        else
        {
            // Reset camera position when player is not moving on ground
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y;
        }
        
        // Set camera position to new position
        m_Camera.transform.localPosition = newCameraPosition;
    }

    private void GetInput(out float speed)
    {
        //float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        //float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool waswalking = m_IsWalking;
        
        m_IsWalking = true;

        //speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        speed = m_WalkSpeed;

        // Store input as a vector.
        m_Input = new Vector2(horizontal, vertical);

        if (Input.GetKeyDown(KeyCode.LeftShift) && m_Input.magnitude > 0f)
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

        // Normalize input if it exceeds 1 in combined length.
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // Update animation blend.
        // _animationBlend = Mathf.Lerp(_animationBlend,  speed * m_Input.sqrMagnitude, Time.deltaTime * 10f);
        // if (_animationBlend < 0.01f) _animationBlend = 0f;

        _animator.SetFloat("Horizontal", horizontal);
        _animator.SetFloat("Vertical", vertical);

       
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }
    
    private void DoDodge(int direction)
    {
        //Check if Animation is Busy
        // if (characterAnimator.AnimationIsBusy())
        //     return;

        switch (direction)
        {
             case 1: // Front
                _animator.SetInteger("DodgeDirection", 1);
                m_Rigidbody.AddForce(transform.forward * 8f, ForceMode.Impulse);
                break;
            case 2: // Right
                _animator.SetInteger("DodgeDirection", 2);
                m_Rigidbody.AddForce(transform.right * 6f, ForceMode.Impulse);
                break;
            case 3: // Left
                _animator.SetInteger("DodgeDirection", 3);
                m_Rigidbody.AddForce(-transform.right * 6f, ForceMode.Impulse);
                break;
            case 4: // Back
                _animator.SetInteger("DodgeDirection", 4);
                m_Rigidbody.AddForce(-transform.forward * 6f, ForceMode.Impulse);
                break;
        }

        //CharacterAnimator ca = GetComponent<CharacterAnimator>();
        //ca.PlayTargetAnimation("Roll");
        //_animator.SetBool("Dodge", true);
        //m_Rigidbody.AddForce(transform.forward * 8f, ForceMode.Impulse);
        
        //Run check dodge function
        StartCoroutine(CheckDodge());
        
    }

    private IEnumerator CheckDodge()
    {
        // ...

        yield return new WaitForSeconds(.1f);

        _animator.SetBool("Dodge", false);
    }

}
