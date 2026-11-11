using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerControlState : MonoBehaviour
{
    [SerializeField] private UICanvasController _inventoryUI;
    [SerializeField] private SettingsMenu _pauseMenu;
    [SerializeField] private ShopInventoryUI _shopUI;
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private StarterAssets.StarterAssetsInputs _inputs;
    private PlayerStats _stats;
    private bool _focused = true;
    private bool _allowed;
    public bool GameplayAllowed => _allowed && Time.timeScale > 0f;
    public event System.Action<bool> GameplayChanged;
    public event System.Action InteractRequested;

    private void Awake()
    {
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        _inputs = GetComponent<StarterAssets.StarterAssetsInputs>();
        _stats = GetComponent<PlayerStats>();
    }
    private void OnEnable() { if (_playerInput != null) _playerInput.onActionTriggered += OnAction; }
    private void Start() => Synchronize();
    private void Update() => Synchronize();
    private void ApplyCursorState()
    {
        CursorLockMode desired = GameplayAllowed ? CursorLockMode.Locked : CursorLockMode.None;
        if (Cursor.lockState != desired) Cursor.lockState = desired;
        if (Cursor.visible == GameplayAllowed) Cursor.visible = !GameplayAllowed;
    }
    private void OnDisable()
    {
        if (_playerInput != null) _playerInput.onActionTriggered -= OnAction;
        _allowed = false;
        if (_inputs != null) { _inputs.move = _inputs.look = Vector2.zero; _inputs.jump = _inputs.sprint = _inputs.toggleCamera = false; }
        GameplayChanged?.Invoke(false);
    }
    private void OnApplicationFocus(bool focused) { _focused = focused; Synchronize(); }
    private bool InteractionOpen => (_inventoryUI != null && _inventoryUI.chestInventoryUI != null && _inventoryUI.chestInventoryUI.gameObject.activeInHierarchy) || (_shopUI != null && _shopUI.gameObject.activeInHierarchy);
    private void OnAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!context.performed || !_focused) return;
        switch (context.action.name)
        {
            case "Pause":
                if (_pauseMenu != null)
                {
                    if (Time.timeScale == 0f) _pauseMenu.Resume();
                    else { ClosePanels(); _pauseMenu.Pause(); }
                }
                break;
            case "Inventory":
                if (Time.timeScale > 0f && _inventoryUI != null) _inventoryUI.ToogleInvetoryUI();
                break;
            case "Interact":
                if (Time.timeScale <= 0f) return;
                if (InteractionOpen) ClosePanels();
                else if (GameplayAllowed) InteractRequested?.Invoke();
                break;
            default:
                return;
        }
        Synchronize();
    }
    public void ClosePanels()
    {
        if (_shopUI != null && _shopUI.gameObject.activeInHierarchy) _shopUI.CloseUI();
        if (_inventoryUI != null)
        {
            if (_inventoryUI.inv != null && _inventoryUI.inv.activeSelf) _inventoryUI.ToogleInvetoryUI();
            else if (_inventoryUI.chestInventoryUI != null && _inventoryUI.chestInventoryUI.gameObject.activeInHierarchy) _inventoryUI.chestInventoryUI.CloseUI();
        }
    }
    public void Synchronize()
    {
        bool allowed = _focused && Time.timeScale > 0f && (_stats == null || !_stats.IsDead) && !InteractionOpen && (_inventoryUI == null || _inventoryUI.inv == null || !_inventoryUI.inv.activeSelf);
        if (allowed != _allowed)
        {
            _allowed = allowed;
            if (_inputs != null) { _inputs.move = _inputs.look = Vector2.zero; _inputs.jump = _inputs.sprint = _inputs.toggleCamera = false; }
            GameplayChanged?.Invoke(allowed);
        }
        ApplyCursorState();
    }
}
