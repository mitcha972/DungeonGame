using UnityEngine;
using TMPro;
using StarterAssets;

/// <summary>Routes player actions to combat, equipment and interactions. Never moves the character.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(PlayerControlState))]
public class PlayerController : MonoBehaviour
{
    public Interactable focus;
    public bool lockedOn => focus != null && focus.CompareTag("Enemy");
    public bool mouseLocked => lockedOn;
    public bool canMove => _movement != null && _movement.canMove;
    [SerializeField] private TMP_Text interactText;
    [SerializeField] private float interactionDistance = 4f;
    public float InteractionDistance => interactionDistance;
    public TMP_Text InteractionText => interactText;
    [Header("Object grabbing")]
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickUpLayerMask;
    private ObjectGrabbable _heldObject;
    private Camera _camera;
    private PlayerCombat _combat;
    private ThirdPersonController _movement;
    private PlayerStats _stats;
    private PlayerGathering _gathering;
    private PlayerControlState _controls;
    private CharacterEquipment _equipment;
    private HotKeysUI _hotKeys;
    private bool GameplayInputAllowed => _controls.GameplayAllowed && (_stats == null || (!_stats.IsDead && _stats.currentHealth > 0));
    private void Awake()
    {
        _camera = Camera.main;
        _combat = GetComponent<PlayerCombat>();
        _movement = GetComponent<ThirdPersonController>();
        _stats = GetComponent<PlayerStats>();
        _gathering = GetComponent<PlayerGathering>();
        _controls = GetComponent<PlayerControlState>();
        _equipment = GetComponent<CharacterEquipment>();
        if (_controls.InventoryUI != null) _hotKeys = _controls.InventoryUI.GetComponentInChildren<HotKeysUI>(true);
    }
    private void OnEnable()
    {
        _controls.GameplayActionRequested += OnAction;
        _controls.InteractRequested += Interact;
        _controls.GameplayChanged += OnGameplayChanged;
    }
    private void OnDisable()
    {
        _controls.GameplayActionRequested -= OnAction;
        _controls.InteractRequested -= Interact;
        _controls.GameplayChanged -= OnGameplayChanged;
        OnGameplayChanged(false);
        RemoveFocus();
        if (_heldObject != null) _heldObject.Drop();
        _heldObject = null;
    }
    private void OnGameplayChanged(bool allowed) { if (!allowed) _combat?.SetShieldBlock(false); }
    private void OnAction(string action)
    {
        if (!GameplayInputAllowed) return;
        switch (action)
        {
            case "Attack": if (_combat != null && !_combat.isBlocking && !_controls.BlockHeld) _combat.Attack(false); break;
            case "Unequip":
                var presentation = GetComponent<CharacterAnimation>();
                if (presentation == null || !presentation.IsBusy) _equipment?.UnequipAllToInventory();
                break;
            case "Grab": ToggleGrab(); break;
            default:
                if (action.StartsWith("QuickSlot") && int.TryParse(action.Substring(9), out int number)) _hotKeys?.UseQuickSlot(number);
                break;
        }
    }
    private void Update()
    {
        _combat?.SetShieldBlock(GameplayInputAllowed && _controls.BlockHeld);
        if (focus == null && _movement != null) _movement.FacingTarget = null;
        UpdateResourcePrompt();
    }
    private Interactable LookedAtInteractable()
    {
        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return null;
        var ray = _camera.ViewportPointToRay(new Vector3(.5f, .5f));
        if (!Physics.Raycast(ray, out var hit, interactionDistance + Vector3.Distance(_camera.transform.position, transform.position), ~(1 << gameObject.layer), QueryTriggerInteraction.Ignore)) return null;
        var target = hit.collider.GetComponentInParent<Interactable>();
        return target != null && target.isActiveAndEnabled && Vector3.Distance(transform.position, target.transform.position) <= interactionDistance ? target : null;
    }
    public ResourceNode LookedAtResource() => LookedAtInteractable() as ResourceNode;
    private void UpdateResourcePrompt()
    {
        if (interactText == null) return;
        if (_gathering != null && _gathering.IsGathering)
        {
            interactText.SetText(_gathering.ActiveNode.definition.actionLabel + "... " + Mathf.RoundToInt(_gathering.Progress * 100) + "%  [Esc: cancel]");
            return;
        }
        if (!GameplayInputAllowed) { interactText.SetText(string.Empty); return; }
        if (_gathering != null && _gathering.HasFeedback) { interactText.SetText(_gathering.Feedback); return; }
        var node = LookedAtResource();
        interactText.SetText(node != null ? node.Prompt : string.Empty);
    }
    private void Interact()
    {
        if (!GameplayInputAllowed) return;
        var target = LookedAtInteractable();
        if (target == null || (target == focus && lockedOn)) { RemoveFocus(); return; }
        RemoveFocus();
        focus = target;
        target.OnFocused(transform);
        target.Interact(transform);
        if (_movement != null) _movement.FacingTarget = lockedOn ? target.transform : null;
    }
    private void RemoveFocus()
    {
        if (focus != null) focus.OnDefocused();
        focus = null;
        if (_movement != null) _movement.FacingTarget = null;
        if (interactText != null) interactText.SetText(string.Empty);
    }
    private void ToggleGrab()
    {
        if (_heldObject != null) { _heldObject.Drop(); _heldObject = null; return; }
        if (_camera == null) _camera = Camera.main;
        if (_camera == null || objectGrabPointTransform == null) return;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit, interactionDistance, pickUpLayerMask, QueryTriggerInteraction.Ignore)
            && hit.collider.TryGetComponent(out ObjectGrabbable grabbable))
        { _heldObject = grabbable; _heldObject.Grab(objectGrabPointTransform); }
    }
    // Kept as public animation/UnityEvent callbacks; movement remains owned by the motor.
    public void EnableMovement() { if (_movement != null && (_stats == null || !_stats.IsDead)) _movement.canMove = _movement.canRotate = true; }
    public void DisableMovement() { if (_movement != null) _movement.canMove = _movement.canRotate = false; }
}
