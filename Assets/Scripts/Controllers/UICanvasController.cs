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


    float XSensitivity ,YSensitivity;

    void Start () 
	{
        //XSensitivity  = FPC.m_MouseLook.XSensitivity;
        //YSensitivity = FPC.m_MouseLook.YSensitivity;
        ToogleInvetoryUI();
    }

    void Update () 
    {
		// Check to see if we should open/close the inventory
		if (Input.GetButtonDown("Inventory")) // Change to main TAb BUtton
			ToogleInvetoryUI();

        /* Main tab is opend with the "e" key, It it closed with the "Escape" key
            - escpe key close the main tap, or if not opend opens the menu screen.
            
            enem currentTab(Game, Player, Menu)
            State machine 

        Mouse
        Checks the tab, If in palyer tab mouse is locked, if in the others mouse is unlocked.

        */

        //chest/shop
        //if (Input.GetKeyDown(KeyCode.I))
        //    chestInventoryUI.CloseUI();

        //player
        //if (Input.GetKeyDown(KeyCode.P))
        //    TooglePlayerUI();
	}

    public void ToogleInvetoryUI()
    {
        bool on = inv.activeSelf;
        
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
            chestInventoryUI.CloseUI();

            tooltip_ItemStats.HideTooltip();
        } 
        else //invemtory open 
        {
            //FPC.m_MouseLook.XSensitivity = 0;
            //FPC.m_MouseLook.YSensitivity = 0;

            Cursor.lockState = CursorLockMode.None;
        }  
    
    }

    public void TooglePlayerUI()
    {
        bool on = playerUI.activeSelf;
        playerUI.SetActive(!on);
    }

    public void TabChange(GameObject newTab)
    {
        if(newTab == currentTab)
            return;
            
        currentTab.SetActive(false);  
        currentTab = newTab;
        currentTab.SetActive(true);  
    }
}
