using UnityEngine;
using TMPro;
using StarterAssets;
using UnityEngine.InputSystem;

/// <summary>Gameplay actions and focus. Locomotion and orbit belong to ThirdPersonController.</summary>
public class PlayerController : MonoBehaviour
{
    public bool lockedOn;
    public Interactable focus;
    public bool canMove;
    public bool mouseLocked;
    [SerializeField] private TMP_Text interactText;
    [SerializeField] private float interactionDistance = 4f;
    public float InteractionDistance => interactionDistance;
    private PlayerGathering _gathering;
    public TMP_Text InteractionText => interactText;
    [SerializeField] private float turnSpeed = 540f;
    private Camera _camera;
    private PlayerCombat _combat;
    private CharacterAnimator _animator;
    private ThirdPersonController _movement;
    private PlayerStats _stats;
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private PlayerControlState _controlState;
    private bool _rightPending, _leftIgnored, _rightIgnored, _rightShield;
    private bool GameplayInputAllowed => _stats != null && !_stats.IsDead && _stats.currentHealth > 0 && (_controlState != null ? _controlState.GameplayAllowed : Time.timeScale > 0f && Cursor.lockState == CursorLockMode.Locked);

    private void Awake()
    {
        _camera = Camera.main;
        _combat = GetComponent<PlayerCombat>();
        _animator = GetComponent<CharacterAnimator>();
        _movement = GetComponent<ThirdPersonController>();
        _stats = GetComponent<PlayerStats>();
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        _controlState = GetComponent<PlayerControlState>();
        _gathering = GetComponent<PlayerGathering>();
        if (_animator != null) { _animator.EnableMovement += EnableMovement; _animator.DisableMovement += DisableMovement; }
    }
    private void OnEnable()
    {
        if (_playerInput != null) _playerInput.onActionTriggered += OnAction;
        if (_controlState != null) { _controlState.InteractRequested += Interact; _controlState.GameplayChanged += ResetCombatInput; }
    }
    private void OnDisable()
    {
        if (_playerInput != null) _playerInput.onActionTriggered -= OnAction;
        if (_controlState != null) { _controlState.InteractRequested -= Interact; _controlState.GameplayChanged -= ResetCombatInput; }
        ResetCombatInput(false);
        RemoveFocus();
    }
    private void OnDestroy()
    {
        if (_animator != null) { _animator.EnableMovement -= EnableMovement; _animator.DisableMovement -= DisableMovement; }
    }
    private void Start() => EnableMovement();
    private void ResetCombatInput(bool allowed)
    {
        _rightPending = _rightShield = false;
        if (_combat != null) _combat.SetShieldBlock(false);
        var actions = _playerInput != null ? _playerInput.actions : null;
        _leftIgnored = actions != null && actions.FindAction("Attack") != null && actions.FindAction("Attack").IsPressed();
        _rightIgnored = actions != null && actions.FindAction("Block") != null && actions.FindAction("Block").IsPressed();
    }
    private void OnAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_combat == null) return;
        if (context.action.name == "Interact" && context.performed && _controlState == null && GameplayInputAllowed) Interact();
        if (context.action.name == "Attack")
        {
            if (context.canceled)
            {
                _leftIgnored = false;
            }
            else if (context.performed && GameplayInputAllowed && !_leftIgnored)
            {
                TryAttack(false);
            }
        }
        if (context.action.name == "Block")
        {
            if (context.canceled)
            {
                _rightPending = _rightShield = _rightIgnored = false;
                _combat.SetShieldBlock(false);
            }
            else if (context.performed && GameplayInputAllowed && !_rightIgnored)
            {
                _rightPending = true;
                _rightShield = _combat.HasShield;
                if (_rightShield) _combat.SetShieldBlock(true);
            }
        }
    }
    private void TryAttack(bool heavy)
    {
        if (GameplayInputAllowed && !_combat.isBlocking && !_rightShield) _combat.Attack(heavy);
    }
    private void Update()
    {
        UpdateResourcePrompt();
        if (!GameplayInputAllowed) { _rightPending = _rightShield = false; _combat?.SetShieldBlock(false); return; }
        if (_rightShield && _rightPending) _combat.SetShieldBlock(true);
        if (focus == null && lockedOn) RemoveFocus();
        if (lockedOn && focus != null && _movement != null && !_movement.FirstPerson && _movement.canRotate)
        {
            Vector3 direction = focus.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }
    }

    public ResourceNode LookedAtResource()
    {
        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return null;
        Ray ray = _camera.ViewportPointToRay(new Vector3(.5f, .5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance + Vector3.Distance(_camera.transform.position, transform.position), ~(1 << gameObject.layer), QueryTriggerInteraction.Ignore)) return null;
        var node = hit.collider.GetComponentInParent<ResourceNode>();
        return node != null && node.isActiveAndEnabled && Vector3.Distance(transform.position, node.transform.position) <= interactionDistance ? node : null;
    }
    private void UpdateResourcePrompt()
    {
        if (interactText == null) return;
        if (_gathering != null && _gathering.IsGathering)
        {
            interactText.SetText(_gathering.ActiveNode.definition.actionLabel + "... " + Mathf.RoundToInt(_gathering.Progress * 100) + "%  [Esc: cancel]"); return;
        }
        if (!GameplayInputAllowed) { interactText.SetText(string.Empty); return; }
        if (_gathering != null && _gathering.HasFeedback) { interactText.SetText(_gathering.Feedback); return; }
        var node = LookedAtResource();
        interactText.SetText(node != null ? node.Prompt : string.Empty);
    }

    private void Interact()
    {
        if (!GameplayInputAllowed || _camera == null) return;
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance + Vector3.Distance(_camera.transform.position, transform.position), ~(1 << gameObject.layer), QueryTriggerInteraction.Ignore))
        {
            var target = hit.collider.GetComponentInParent<Interactable>();
            if (target != null && target.isActiveAndEnabled && Vector3.Distance(transform.position, target.transform.position) <= interactionDistance)
            {
                SetFocus(target);
                return;
            }
        }
        RemoveFocus();
    }

    private void SetFocus(Interactable target)
    {
        if (focus == target && lockedOn) { RemoveFocus(); return; }
        RemoveFocus();
        focus = target;
        target.OnFocused(transform);
        target.Interact(transform);
        lockedOn = target.CompareTag("Enemy");
        mouseLocked = lockedOn;
        if (_movement != null) _movement.SetRotateOnMove(!lockedOn);
    }

    private void RemoveFocus()
    {
        if (focus != null) focus.OnDefocused();
        focus = null;
        lockedOn = mouseLocked = false;
        if (_movement != null) _movement.SetRotateOnMove(true);
        if (interactText != null) interactText.SetText(string.Empty);
    }

    public void EnableMovement()
    {
        if (_stats != null && _stats.IsDead) return;
        canMove = true;
        if (_movement == null) return;
        _movement.canMove = true;
        _movement.canRotate = true;
    }
    public void DisableMovement()
    {
        canMove = false;
        if (_movement == null) return;
        _movement.canMove = false;
        _movement.canRotate = false;
    }
}
