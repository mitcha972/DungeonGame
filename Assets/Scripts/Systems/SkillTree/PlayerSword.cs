using System;
using System.Collections.Generic;
using UnityEngine;
//using V_AnimationSystem;
//using CodeMonkey.Utils;
//using CodeMonkey.MonoBehaviours;

public class PlayerSword : MonoBehaviour{
    
    public static PlayerSword instance;

    private Player player;
    private State state;
    //private float speed = 50f;
    private Material material;
    private Color materialTintColor;
    //private World_Bar healthBar;

    //Check Skills//
    public UI_SkillTree UITree;

    private enum State {
        Normal,
        Attacking,
        Busy,
    }

    private void Update() {
       
        if (Input.GetKeyDown(KeyCode.E)) { //attack key

            //Check what skill is equipted/////

            if (UI_SkillTree.selectedSkill.ToString() == "Earthshatter")
            {
                
            }

            if (UI_SkillTree.selectedSkill.ToString() == "Whirlwind")
            {
                
            }

            if (UI_SkillTree.selectedSkill.ToString() == "MoveSpeed_1")
            {
                Debug.Log("Used Skill " + UI_SkillTree.selectedSkill.ToString());
            }

            if (UI_SkillTree.selectedSkill.ToString() == "MoveSpeed_2")
            {
                Debug.Log("Used Skill " + UI_SkillTree.selectedSkill.ToString());
            }

            if (UI_SkillTree.selectedSkill.ToString() == "HealthMax_1")
            {
                Debug.Log("Used Skill " + UI_SkillTree.selectedSkill.ToString());
            }

            if (UI_SkillTree.selectedSkill.ToString() == "HealthMax_2")
            {
                Debug.Log("Used Skill " + UI_SkillTree.selectedSkill.ToString());
            }
        }
    }
    
}
