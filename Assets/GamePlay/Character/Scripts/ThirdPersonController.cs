using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour
	{
        public bool canMove, canRotate;
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
        public float Sensitivity = 1f;
        [SerializeField] private float _braking = 50f;
        [SerializeField] private float _airAcceleration = 15f;
        [SerializeField] private float _jumpBuffer = 0.1f;
        [SerializeField] private float _coyoteTime = 0.08f;
        private Vector3 _horizontalVelocity;
        private float _lastGroundTime = float.NegativeInfinity;
        private float _jumpPressTime = float.NegativeInfinity;
        private bool _jumpConsumed;
        private PlayerControlState _controlState;
        public bool FirstPerson { get; set; }
        public float ViewYaw => _cinemachineTargetYaw;
        private bool InputAllowed => _controlState != null ? _controlState.GameplayAllowed : Cursor.lockState == CursorLockMode.Locked;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;
		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float CameraAngleOverride = 0.0f;
		[Tooltip("For locking the camera position on all axis")]
		public bool LockCameraPosition = false;

		// cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

		private Animator _animator;
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private UnityEngine.InputSystem.PlayerInput _playerInput;
		private UnityEngine.InputSystem.InputAction _lookAction;
		private GameObject _mainCamera;
        private bool _rotateOnMove = true;
		private const float _threshold = 0.01f;

		private bool _hasAnimator;

		private void Awake()
		{
			_mainCamera = Camera.main != null ? Camera.main.gameObject : null;
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
			_controlState = GetComponent<PlayerControlState>();
            _lookAction = _playerInput != null && _playerInput.actions != null ? _playerInput.actions.FindAction("Look") : null;
		}

		private void Start()
		{
			if (_mainCamera == null || _input == null || CinemachineCameraTarget == null)
			{
				Debug.LogError("Third-person movement requires a main camera, input and camera target.", this);
				enabled = false;
				return;
			}
			_cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;
			_cinemachineTargetPitch = Mathf.DeltaAngle(0f, CinemachineCameraTarget.transform.eulerAngles.x);
			AssignAnimationIDs();
			_jumpTimeoutDelta = 0f;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
            if (!_controller.enabled || Time.timeScale <= 0f) return;
            CameraRotation();
            if (_input.jump && InputAllowed && canMove) _jumpPressTime = Time.time;
            _input.jump = false;
            if (!InputAllowed || !canMove) _jumpPressTime = float.NegativeInfinity;
            GroundedCheck();
            JumpAndGravity();
            Move();
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

        private void GroundedCheck()
        {
            Vector3 foot = transform.TransformPoint(_controller.center) - Vector3.up * (_controller.height * transform.lossyScale.y * 0.5f);
            float radius = Mathf.Min(GroundedRadius, _controller.radius * 0.9f);
            Vector3 origin = foot + Vector3.up * (radius + _controller.skinWidth + 0.04f);
            Grounded = _verticalVelocity <= 0f && Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, _controller.skinWidth + 0.08f, GroundLayers, QueryTriggerInteraction.Ignore) && !hit.collider.transform.IsChildOf(transform) && Vector3.Angle(hit.normal, Vector3.up) <= _controller.slopeLimit;
            if (Grounded) { _lastGroundTime = Time.time; _jumpConsumed = false; }
            if (_hasAnimator) _animator.SetBool(_animIDGrounded, Grounded);
        }

		private void CameraRotation()
		{
			// if there is an input and camera position is not fixed
			if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition && InputAllowed && Time.timeScale > 0f)
			{
				bool mouseLook = _lookAction != null && _lookAction.activeControl != null && _lookAction.activeControl.device is UnityEngine.InputSystem.Mouse;
				float multiplier = mouseLook ? 1f : Time.deltaTime;
				_cinemachineTargetYaw += _input.look.x * multiplier * (mouseLook ? Sensitivity : 1f);
                _cinemachineTargetPitch -= _input.look.y * multiplier * (mouseLook ? Sensitivity : 1f);
            }

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            if (FirstPerson && canRotate && InputAllowed) transform.rotation = Quaternion.Euler(0f, _cinemachineTargetYaw, 0f);
			// Cinemachine will follow this target
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		}

        private void Move()
        {
            bool allowed = InputAllowed && canMove;
            Vector2 move = allowed ? Vector2.ClampMagnitude(_input.move, 1f) : Vector2.zero;
            float magnitude = _input.analogMovement ? move.magnitude : (move.sqrMagnitude > 0.0001f ? 1f : 0f);
            float targetSpeed = (_input.sprint ? SprintSpeed : MoveSpeed) * magnitude;
            Vector3 direction = Quaternion.Euler(0f, _cinemachineTargetYaw, 0f) * new Vector3(move.x, 0f, move.y).normalized;
            Vector3 desired = direction * targetSpeed;
            float acceleration = magnitude == 0f || Vector3.Dot(_horizontalVelocity, desired) < 0f ? _braking : SpeedChangeRate;
            if (!Grounded && magnitude > 0f) acceleration = _airAcceleration;
            _horizontalVelocity = allowed ? Vector3.MoveTowards(_horizontalVelocity, desired, acceleration * Time.deltaTime) : Vector3.zero;
            if (magnitude > 0f && _rotateOnMove && !FirstPerson && canRotate)
            {
                _targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
            Vector3 start = transform.position;
            CollisionFlags flags = CollisionFlags.None;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Time.deltaTime / (1f / 60f)));
            float step = Time.deltaTime / steps;
            for (int i = 0; i < steps; i++)
            {
                float nextVertical = Mathf.Max(_verticalVelocity + Gravity * step, -_terminalVelocity);
                CollisionFlags stepFlags = _controller.Move(_horizontalVelocity * step + Vector3.up * ((_verticalVelocity + nextVertical) * 0.5f * step));
                _verticalVelocity = nextVertical;
                flags |= stepFlags;
                if ((stepFlags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f) _verticalVelocity = 0f;
                if ((stepFlags & CollisionFlags.Below) != 0 && _verticalVelocity < 0f) _verticalVelocity = -2f;
            }
            if ((flags & CollisionFlags.Sides) != 0 && Time.deltaTime > 0f)
            {
                Vector3 actual = (transform.position - start) / Time.deltaTime;
                _horizontalVelocity = new Vector3(actual.x, 0f, actual.z);
            }
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0f);
            _speed = _horizontalVelocity.magnitude;
            _animationBlend= Mathf.Lerp(_animationBlend, _speed, 1f - Mathf.Exp(-SpeedChangeRate * Time.deltaTime));
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, magnitude);
            }
        }

        private void JumpAndGravity()
        {
            _jumpTimeoutDelta -= Time.deltaTime;
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_verticalVelocity < 0f) _verticalVelocity = -2f;
                if (_hasAnimator) { _animator.SetBool(_animIDJump, false); _animator.SetBool(_animIDFreeFall, false); }
            }
            else
            {
                _fallTimeoutDelta -= Time.deltaTime;
                if (_fallTimeoutDelta <= 0f && _hasAnimator) _animator.SetBool(_animIDFreeFall, true);
            }
            if (InputAllowed && canMove && !_jumpConsumed && Time.time - _lastGroundTime <= _coyoteTime && Time.time - _jumpPressTime <= _jumpBuffer && _jumpTimeoutDelta <= 0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _jumpPressTime = float.NegativeInfinity;
                _jumpConsumed = true;
                Grounded = false;
                _jumpTimeoutDelta = JumpTimeout;
                if (_hasAnimator) { _animator.SetBool(_animIDJump, true); _animator.SetBool(_animIDGrounded, false); }
            }
            }

      private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

        public void SetSensitivity(float newSensitivity) 
        {
            Sensitivity = newSensitivity;
        }

        public void SetRotateOnMove(bool newRotateOnMove) 
        {
            _rotateOnMove = newRotateOnMove;
        }
	}
}