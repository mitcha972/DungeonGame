using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Static script that keeps track of game info//
public class GameController : MonoBehaviour
{
    //Player stats
    public static string playerName;
    public PlayerContent playerContent;
    public static Class playerClass;
    public int m_floor;
    public int m_difficulty;
    public static int floor;
    public static int difficulty;
    public static int goToScene;

    void Awake()
    {        
        //If game scene
        if ( SceneManager.GetActiveScene().name != "Menu")
        {
            playerContent.SetUpPlayer(playerName, playerClass);
        }

        floor = m_floor;
        difficulty = m_difficulty;
    }

    public void MenuStart(Class pClass)
    { 
        playerName = "Player";
        playerClass = pClass;
        playerContent.SetUpPlayer(playerName, playerClass);
    }
}
