using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameController GC;
    public Class pClass;
    public string name;
    public TMP_Text classDescriptionText;
    public TMP_Text classNameText;
    public TMP_Text classDamageText, classArmorText;

    void Start()
    {
        GC.MenuStart(pClass);
        classDescriptionText.text = pClass.description;
        classNameText.text = pClass.name;
        classDamageText.text =  ""+GC.playerContent.player.damage;
        classArmorText.text =  ""+GC.playerContent.player.armor;
    }

    public void NewClass(Class newClass)
    {
        if (newClass == null) return;
        pClass = newClass;
        GameController.playerClass = newClass;
        GC.playerContent.player.equipmentManager.UnequipAll();
        classDescriptionText.text = pClass.description;
        classNameText.text = pClass.name + ":";
        GC.playerContent.NewClass(pClass);

        //Update equiptemnet stats
        classDamageText.text =  ""+GC.playerContent.player.damage;
        classArmorText.text =  ""+GC.playerContent.player.armor;
    }    

    public void UpdateName(Text newName)
    {
        name = newName.text;
    }

    private bool _startingGame;
    public void Play()
    {
        if (_startingGame || pClass == null) return;
        int target = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Game/Main.unity");
        int loading = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Game/Loading.unity");
        if (target < 0 || loading < 0) { Debug.LogError("DungeonFloorTest and Loading must be enabled in Build Settings.", this); return; }
        _startingGame = true;
        GameController.goToScene = target;
        GameController.playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name;
        GameController.playerClass = pClass;
        Time.timeScale = 1f;
        SceneManager.LoadScene(loading);
    }

}
