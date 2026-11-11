using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool toggleCamera;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnToggleCamera(InputValue value)
		{
			ToggleCameraInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void ToggleCameraInput(bool newToggleState)
		{
			toggleCamera = newToggleState;
		}

		private UnityEngine.InputSystem.PlayerInput _playerInput;
		private PlayerControlState _state;
		private void Awake() { _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>(); _state = GetComponent<PlayerControlState>(); }
		private void OnEnable() { if (_playerInput != null) _playerInput.onActionTriggered += OnAction; }
		private void OnDisable() { if (_playerInput != null) _playerInput.onActionTriggered -= OnAction; ResetInput(); }
		public void ResetInput() { move = look = Vector2.zero; jump = sprint = toggleCamera = false; }
		private void OnAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
		{
		    bool allowed = _state != null ? _state.GameplayAllowed : Cursor.lockState == CursorLockMode.Locked && Time.timeScale > 0f;
		    switch (context.action.name)
		    {
		        case "Move": MoveInput(allowed && !context.canceled ? context.ReadValue<Vector2>() : Vector2.zero); break;
		        case "Look": LookInput(allowed && cursorInputForLook && !context.canceled ? context.ReadValue<Vector2>() : Vector2.zero); break;
		        case "Sprint": SprintInput(allowed && context.ReadValue<float>() > 0.5f); break;
		        case "Jump": if (allowed && context.performed) jump = true; break;
		        case "ToggleCamera": if (allowed && context.performed) toggleCamera = true; break;
		    }
		}
		private void OnApplicationFocus(bool hasFocus)
		{
		    if (!hasFocus) ResetInput();
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}