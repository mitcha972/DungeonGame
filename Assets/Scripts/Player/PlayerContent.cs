using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContent : MonoBehaviour
{
    public PlayerStats player;

    public void SetUpPlayer(string name, Class playerClass)
    {
        //Setup player stats
        //Debug.Log("PCWORKEDDDD");
        player.name = name;
        player.playerClass = playerClass;
    }

    public void NewClass(Class playerClass)
    {
        player.playerClass = playerClass;
        player.SetUpEquipment();
    }

}
