using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerControlState : MonoBehaviour
{
    [SerializeField] private UICanvasController _inventoryUI;
    [SerializeField] private SettingsMenu _pauseMenu;
    [SerializeField] private ShopInventoryUI _shopUI;
    [SerializeField] private DungeonGame.Dialogue.DialogueController _dialogue;
    [SerializeField] private DungeonGame.Quests.QuestJournal _questJournal;
    public ShopInventoryUI ShopUI => _shopUI;
    [SerializeField] private MapController _map;
    public bool DialogueOpen => _dialogue != null && _dialogue.IsOpen;
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    public UICanvasController InventoryUI => _inventoryUI;
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Sprint { get; private set; }
    public bool BlockHeld { get; private set; }
    public bool LookIsMouse { get; private set; }
    [SerializeField] private bool analogMovement;
    public bool AnalogMovement => analogMovement;
    public event System.Action<string> GameplayActionRequested;
    private readonly System.Collections.Generic.HashSet<UnityEngine.InputSystem.InputAction> suppressedButtons = new System.Collections.Generic.HashSet<UnityEngine.InputSystem.InputAction>();
    private PlayerStats _stats;
    private bool _focused = true;
    private bool _allowed;
    public bool GameplayAllowed => _allowed && Time.timeScale > 0f;
    private UnityEngine.Object _actionOwner;
    public bool ActionContextAllowed { get; private set; }
    public bool ActionLocked => _actionOwner != null;
    public event System.Action ActionCancelRequested;
    public bool TryAcquireAction(UnityEngine.Object owner)
    {
        Synchronize();
        if (owner == null || !GameplayAllowed || ActionLocked) return false;
        _actionOwner = owner; Synchronize(); return true;
    }
    public void ReleaseAction(UnityEngine.Object owner)
    {
        if (_actionOwner != owner) return;
        _actionOwner = null; Synchronize();
    }
    public event System.Action<bool> GameplayChanged;
    public event System.Action InteractRequested;

    private void Awake()
    {
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        _stats = GetComponent<PlayerStats>();
        if (_dialogue == null) _dialogue = GetComponent<DungeonGame.Dialogue.DialogueController>();
    }
    private void OnEnable() { if (_playerInput != null) _playerInput.onActionTriggered += OnAction; }
    private void Start() => Synchronize();
    private void Update() => Synchronize();
    private void ApplyCursorState()
    {
        bool lockCursor = GameplayAllowed || (ActionLocked && ActionContextAllowed);
        CursorLockMode desired = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        if (Cursor.lockState != desired) Cursor.lockState = desired;
        if (Cursor.visible == lockCursor) Cursor.visible = !lockCursor;
    }
    private void OnDisable()
    {
        if (_playerInput != null) _playerInput.onActionTriggered -= OnAction;
        _allowed = false;
        ActionContextAllowed = false;
        ActionCancelRequested?.Invoke();
        ResetIntent();
        GameplayChanged?.Invoke(false);
    }
    private void OnApplicationFocus(bool focused) { _focused = focused; Synchronize(); }
    private bool InteractionOpen => (_inventoryUI != null && _inventoryUI.chestInventoryUI != null && _inventoryUI.chestInventoryUI.gameObject.activeInHierarchy) || (_shopUI != null && _shopUI.gameObject.activeInHierarchy);
    private void OnAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Synchronize();
        string action = context.action.name;
        if (context.canceled)
        {
            suppressedButtons.Remove(context.action);
            if (action == "Move") Move = Vector2.zero;
            if (action == "Look") Look = Vector2.zero;
            if (action == "Sprint") Sprint = false;
            if (action == "Block") BlockHeld = false;
            return;
        }
        if (!context.performed || !_focused) return;
        switch (action)
        {
            case "Move": Move = GameplayAllowed ? context.ReadValue<Vector2>() : Vector2.zero; return;
            case "Look":
                Look = GameplayAllowed ? context.ReadValue<Vector2>() : Vector2.zero;
                LookIsMouse = context.control != null && context.control.device is UnityEngine.InputSystem.Mouse;
                return;
            case "Sprint": Sprint = GameplayAllowed && context.ReadValue<float>() > .5f; return;
            case "Block": BlockHeld = GameplayAllowed && !suppressedButtons.Contains(context.action) && context.ReadValue<float>() > .5f; return;
        }
        if (ActionLocked)
        {
            if (context.action.name == "Pause") ActionCancelRequested?.Invoke();
            return;
        }
        if (_map != null && _map.IsExpanded)
        {
            if (context.action.name == "Pause" || context.action.name == "Map") { _map.SetExpanded(false); Synchronize(); }
            return;
        }
        if (_questJournal != null && _questJournal.IsOpen)
        {
            if (context.action.name == "Pause" || context.action.name == "Journal") _questJournal.Close();
            return;
        }
        if (DialogueOpen)
        {
            if (context.action.name == "Pause") _dialogue.Close();
            return;
        }
        switch (context.action.name)
        {
            case "Map":
                if (GameplayAllowed && _map != null) _map.SetExpanded(true);
                break;
            case "Journal":
                if (GameplayAllowed && _questJournal != null) _questJournal.Open();
                break;
            case "Pause":
                if (InteractionOpen) { ClosePanels(); break; }
                if (_pauseMenu != null)
                {
                    if (Time.timeScale == 0f) _pauseMenu.Resume();
                    else { ClosePanels(); _pauseMenu.Pause(); }
                }
                break;
            case "Inventory":
                if (InteractionOpen) return;
                if (Time.timeScale > 0f && _inventoryUI != null) _inventoryUI.ToogleInvetoryUI();
                break;
            case "Interact":
                if (Time.timeScale <= 0f) return;
                if (InteractionOpen) ClosePanels();
                else if (GameplayAllowed) InteractRequested?.Invoke();
                break;
            default:
                if (GameplayAllowed && !suppressedButtons.Contains(context.action)) GameplayActionRequested?.Invoke(action);
                return;
        }
        Synchronize();
    }
    public void ClosePanels()
    {
        if (_map != null) _map.SetExpanded(false);
        if (_questJournal != null) _questJournal.Close();
        if (DialogueOpen) _dialogue.Close();
        if (_shopUI != null && _shopUI.gameObject.activeInHierarchy) _shopUI.CloseUI();
        if (_inventoryUI != null)
        {
            if (_inventoryUI.inv != null && _inventoryUI.inv.activeSelf) _inventoryUI.ToogleInvetoryUI();
            else if (_inventoryUI.chestInventoryUI != null && _inventoryUI.chestInventoryUI.gameObject.activeInHierarchy) _inventoryUI.chestInventoryUI.CloseUI();
        }
    }
    public void Synchronize()
    {
        bool allowed = isActiveAndEnabled && _focused && Time.timeScale > 0f && (_map == null || !_map.IsExpanded) && !DialogueOpen && (_questJournal == null || !_questJournal.IsOpen) && (_stats == null || !_stats.IsDead) && !InteractionOpen && (_inventoryUI == null || _inventoryUI.inv == null || !_inventoryUI.inv.activeSelf);
        ActionContextAllowed = allowed;
        allowed = allowed && !ActionLocked;
        if (allowed != _allowed)
        {
            _allowed = allowed;
            ResetIntent();
            GameplayChanged?.Invoke(allowed);
        }
        ApplyCursorState();
    }
    private void ResetIntent()
    {
        Move = Look = Vector2.zero;
        Sprint = BlockHeld = false;
        if (_playerInput == null || _playerInput.actions == null) return;
        // A button held while a modal/focus boundary changes must be released before reuse.
        foreach (var action in _playerInput.actions)
            if (action.type == UnityEngine.InputSystem.InputActionType.Button && action.IsPressed()) suppressedButtons.Add(action);
    }
}
