using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem 
{
    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    private static readonly int[] experiencePerLevel = new[] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };

    private int level;
    private int experience;

    public LevelSystem() 
    {
        level = 0;
        experience = 0;
    }

    public void AddExperience(int amount) 
    {
        if (amount <= 0) return;
        if (!IsMaxLevel()) {
            long remaining = (long)experience + amount;
            while (!IsMaxLevel() && remaining >= GetExperienceToNextLevel(level)) {
                // Enough experience to level up
                remaining -= GetExperienceToNextLevel(level);
                level++;
                experience = (int)System.Math.Min(int.MaxValue, remaining);
                if (OnLevelChanged != null) OnLevelChanged(this, EventArgs.Empty);
            }
            experience = IsMaxLevel() ? 0 : (int)remaining;
            if (OnExperienceChanged != null) OnExperienceChanged(this, EventArgs.Empty);
        }
    }

    public int GetLevelNumber() 
    {
        return level;
    }

    public float GetExperienceNormalized() 
    {
        if (IsMaxLevel()) {
            return 1f;
        } else {
            return (float)experience / GetExperienceToNextLevel(level);
        }
    }

    public int GetExperience() 
    {
        return experience;
    }

    public int GetExperienceToNextLevel(int level) 
    {
        if (level >= 0 && level < experiencePerLevel.Length) {
            return experiencePerLevel[level];
        } else {
            // Level Invalid
            Debug.LogError("Level invalid: " + level);
            return 100;
        }
    }

    public bool IsMaxLevel() 
    {
        return IsMaxLevel(level);
    }

    public bool IsMaxLevel(int level) 
    {
        return level == experiencePerLevel.Length - 1;
    }
}
