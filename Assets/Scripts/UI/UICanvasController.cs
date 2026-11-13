using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvasController : MonoBehaviour
{
    [SerializeField] private GameObject currentTab;
    [SerializeField] private PlayerController FPC;
    public GameObject inv;
    [SerializeField] private GameObject background;
    public ChestInventoryUI chestInventoryUI;
    [SerializeField] private GameObject playerUI;

    [SerializeField] private Tooltip_ItemStats tooltip_ItemStats;


    [SerializeField] private PlayerControlState _controlState;
    private UnityEngine.InputSystem.InputAction _inventoryAction;

    void Awake()
    {
        if (_controlState != null) return;
        _inventoryAction = new UnityEngine.InputSystem.InputAction("Inventory", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/tab");
        UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(_inventoryAction, "<Gamepad>/select");
        _inventoryAction.performed += OnInventory;
    }
    void OnEnable() => _inventoryAction?.Enable();
    void OnDisable() => _inventoryAction?.Disable();
    void OnDestroy() => _inventoryAction?.Dispose();
    private void OnInventory(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0f) ToogleInvetoryUI();
    }

    void Start ()
    {
        if (inv != null) inv.SetActive(false);
        if (background != null) background.SetActive(false);
        if (_controlState == null) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    public void ToogleInvetoryUI()
    {
        if (inv == null || background == null) return;
        bool on = inv.activeSelf;
        Cursor.visible = !on;
        
        //Screen.lockCursor = !Screen.lockCursor;
        //Cursor.visible = !on;
        inv.SetActive(!on);
        background.SetActive(!on);

        //Lock Player Rotation
        if(on) //Inventory close 
        {
            //FPC.m_MouseLook.XSensitivity = XSensitivity;
            //FPC.m_MouseLook.YSensitivity = YSensitivity; 
            Cursor.lockState = CursorLockMode.Locked;

            //remove chest ui
            if (chestInventoryUI != null) chestInventoryUI.CloseUI();

            if (tooltip_ItemStats != null) tooltip_ItemStats.HideTooltip();
        } 
        else //invemtory open 
        {
            //FPC.m_MouseLook.XSensitivity = 0;
            //FPC.m_MouseLook.YSensitivity = 0;

            Cursor.lockState = CursorLockMode.None;
        }
        if (_controlState != null) _controlState.Synchronize();
    
    }

    public void TooglePlayerUI()
    {
        if (playerUI == null) return;
        bool on = playerUI.activeSelf;
        playerUI.SetActive(!on);
    }

    public void TabChange(GameObject newTab)
    {
        if(newTab == null || newTab == currentTab)
            return;
            
        if (currentTab != null) currentTab.SetActive(false);  
        currentTab = newTab;
        currentTab.SetActive(true);  
    }
}
