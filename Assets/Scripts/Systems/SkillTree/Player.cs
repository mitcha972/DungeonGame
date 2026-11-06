using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Move to Player Stats!!!/
public class Player : MonoBehaviour 
{
    [SerializeField] private ExperienceBar experienceBar;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;

    private PlayerSword playerSword;// skills
    private LevelSystem levelSystem;
    private LevelSystemAnimated levelSystemAnimated;
    private PlayerSkills playerSkills; 

    public UICanvasController uiCanvasController;

    private void Awake() 
    {
        playerSword = GetComponent<PlayerSword>();//Change to skills

        levelSystem = new LevelSystem();
        levelSystemAnimated = new LevelSystemAnimated(levelSystem);

        playerSkills = new PlayerSkills();
        //playerSkills.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;
    }

    private void Start() 
    {
        levelSystemAnimated.OnExperienceChanged += LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged += LevelSystemAnimated_OnLevelChanged;
        levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        //levelSystem.AddExperience(300);
    }

    public PlayerSkills GetPlayerSkills() 
    {
        return playerSkills;
    }

    private void LevelSystemAnimated_OnLevelChanged(object sender, System.EventArgs e) 
    {
        // Level Up
        levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        playerSkills.AddSkillPoint();

        //Change Health
        //
    }

    private void LevelSystemAnimated_OnExperienceChanged(object sender, System.EventArgs e) 
    {
        experienceBar.SetSize(levelSystemAnimated.GetExperienceNormalized());
    }

/*
    When We kill can enemy
    private void PlayerSword_OnEnemyKilled(object sender, System.EventArgs e) 
    {
        levelSystem.AddExperience(30);
    }
*/

}
