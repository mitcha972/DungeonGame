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
        pClass = newClass;
        classDescriptionText.text = pClass.description;
        classNameText.text = (pClass.name + ":");
        GC.playerContent.NewClass(pClass);

        //Update equiptemnet stats
        classDamageText.text =  ""+GC.playerContent.player.damage;
        classArmorText.text =  ""+GC.playerContent.player.armor;
    }    

    public void UpdateName(Text newName)
    {
        name = newName.text;
    }

    public void Play()
    {
        //Go to Game Scene
        GameController.goToScene = 2;
        GameController.playerName = name;
        SceneManager.LoadScene("Loading");
    }

}
