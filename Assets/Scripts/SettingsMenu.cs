using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    bool isOpen;
    public Transform Tabs;
    int currentTab;

    void Awake()
    {
        currentTab = 0;
        isOpen = false;
    }

    void Toogle()
    {
        if(isOpen)//if is open
        {
            //clsoe all pages
            for(int i = 0; i < this.transform.childCount; i++)
                this.transform.GetChild(i).gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.None;   //Change to global Lock Curosor //Check is needs to be locked        
        }
        else
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
       
        isOpen = !isOpen;
    }

    // Update is called once per frame
    void Update()
    {
        //if Key down
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("toogle");
            Toogle();
        }            
    }

    public void NewTab(int newTab)
    {
        //if New Tab down
        if(newTab == currentTab) 
            return;

        Tabs.GetChild(currentTab).gameObject.SetActive(false);
        currentTab = newTab;
        Tabs.GetChild(currentTab).gameObject.SetActive(true);
    }

    public void Exit()
    {
        //Go Back to Main Menu
        //
    }
}
