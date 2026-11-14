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
        string sceneName = gameObject.scene.name;
        // Loading carries the shared controller prefab but no player preview or gameplay owner.
        if (sceneName == "Loading") return;
        if (sceneName == "Start" || sceneName == "Menu")
        {
            floor = m_floor;
            difficulty = m_difficulty;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        if (playerContent == null)
        {
            Debug.LogError("GameController requires the scene PlayerContent reference.", this);
            return;
        }
        if (playerClass == null && playerContent.player != null) playerClass = playerContent.player.playerClass;
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "Player";
        playerContent.SetUpPlayer(playerName, playerClass);
    }

    public void MenuStart(Class pClass)
    { 
        playerName = "Player";
        playerClass = pClass;
        playerContent.SetUpPlayer(playerName, playerClass);
    }
}
