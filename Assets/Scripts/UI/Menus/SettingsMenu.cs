using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    private bool isOpen;
    public Transform Tabs;
    private int currentTab;
    [SerializeField] private PlayerControlState _controlState;
    private UnityEngine.InputSystem.InputAction _pause;
    private UICanvasController _inventoryUI;

    private void Awake()
    {
        _inventoryUI = GetComponentInParent<UICanvasController>();
        if (_controlState != null) return;
        _pause = new UnityEngine.InputSystem.InputAction("Pause", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/escape");
        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(_pause, "<Gamepad>/start");
        _pause.performed += OnPause;
    }
    private void OnEnable() => _pause?.Enable();
    private void OnDisable() { _pause?.Disable(); if (isOpen) Time.timeScale = 1f; }
    private void OnDestroy() => _pause?.Dispose();
    private void OnPause(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (_inventoryUI != null && _inventoryUI.inv != null && _inventoryUI.inv.activeSelf)
        {
            _inventoryUI.ToogleInvetoryUI();
            return;
        }
        if (isOpen) Resume(); else Pause();
    }
    public void Pause()
    {
        isOpen = true;
        if (transform.childCount > 0) transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_controlState != null) _controlState.Synchronize();
    }
    public void Resume()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(false);
        isOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (_controlState != null) _controlState.Synchronize();
    }
    public void NewTab(int newTab)
    {
        if (Tabs == null || newTab < 0 || newTab >= Tabs.childCount || newTab == currentTab) return;
        if (currentTab < Tabs.childCount) Tabs.GetChild(currentTab).gameObject.SetActive(false);
        currentTab = newTab;
        Tabs.GetChild(currentTab).gameObject.SetActive(true);
    }
    public void Exit()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }
}

