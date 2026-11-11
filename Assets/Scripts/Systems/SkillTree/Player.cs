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
    private PlayerStats _stats;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
        if (_stats != null) return;
        playerSword = GetComponent<PlayerSword>();//Change to skills

        levelSystem = new LevelSystem();
        levelSystemAnimated = new LevelSystemAnimated(levelSystem, this);

        playerSkills = new PlayerSkills();
        //playerSkills.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;
    }

    private void OnDestroy()
    {
        if (levelSystemAnimated == null) return;
        levelSystemAnimated.OnExperienceChanged -= LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged -= LevelSystemAnimated_OnLevelChanged;
        levelSystemAnimated.Dispose();
    }

    private void Start()
    {
        if (_stats != null) return;
        levelSystemAnimated.OnExperienceChanged += LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged += LevelSystemAnimated_OnLevelChanged;
        if (levelText != null) levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        //levelSystem.AddExperience(300);
    }

    public PlayerSkills GetPlayerSkills()
    {
        return _stats != null ? _stats.GetPlayerSkills() : playerSkills;
    }

    private void LevelSystemAnimated_OnLevelChanged(object sender, System.EventArgs e) 
    {
        // Level Up
        if (levelText != null) levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        playerSkills.AddSkillPoint();

        //Change Health
        //
    }

    private void LevelSystemAnimated_OnExperienceChanged(object sender, System.EventArgs e) 
    {
        if (experienceBar != null) experienceBar.SetSize(levelSystemAnimated.GetExperienceNormalized());
    }

/*
    When We kill can enemy
    private void PlayerSword_OnEnemyKilled(object sender, System.EventArgs e) 
    {
        levelSystem.AddExperience(30);
    }
*/

}
