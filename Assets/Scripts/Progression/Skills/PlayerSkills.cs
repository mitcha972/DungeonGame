/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills {

    public event EventHandler OnSkillPointsChanged;
    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillUnlocked;
    public class OnSkillUnlockedEventArgs : EventArgs {
        public SkillType skillType;
    }

    public enum SkillType {
        None,
        Earthshatter,
        Whirlwind,
        MoveSpeed_1,
        MoveSpeed_2,
        HealthMax_1,
        HealthMax_2,
    }

    private List<SkillType> unlockedSkillTypeList;
    private int skillPoints;

    public PlayerSkills() {
        unlockedSkillTypeList = new List<SkillType>();
    }

    public void AddSkillPoint() {
        skillPoints++;
        OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetSkillPoints() {
        return skillPoints;
    }

    private void UnlockSkill(SkillType skillType) {
        if (!IsSkillUnlocked(skillType)) {
            unlockedSkillTypeList.Add(skillType);
            OnSkillUnlocked?.Invoke(this, new OnSkillUnlockedEventArgs { skillType = skillType });
        }
    }

    public bool IsSkillUnlocked(SkillType skillType) {
        return unlockedSkillTypeList.Contains(skillType);
    }

    public bool CanUnlock(SkillType skillType)
    {
        // Existing paths are alternatives: any unlocked prerequisite is sufficient.
        SkillType[] requirements = GetSkillRequirement(skillType);
        if (requirements.Length == 0 || requirements[0] == SkillType.None) return true;
        foreach (SkillType requirement in requirements)
            if (requirement != SkillType.None && IsSkillUnlocked(requirement)) return true;
        return false;
    }

    public SkillType[] GetSkillRequirement(SkillType skillType) {
        SkillType[] st = new SkillType[2];
        st[0] = SkillType.None;
        st[1] = SkillType.None;

        switch (skillType) 
        {
            case SkillType.HealthMax_2:     st[0] = SkillType.HealthMax_1; return st;
            case SkillType.MoveSpeed_2:     st[0] = SkillType.MoveSpeed_1; return st;
            case SkillType.Earthshatter:    st[0] = SkillType.Whirlwind; return st;
            case SkillType.Whirlwind:       st[0] = SkillType.MoveSpeed_2; st[1] = SkillType.HealthMax_2; return st;
            //case SkillType.Whirlwind:       return SkillType.HealthMax_2; 
        }
        //st[0] = SkillType.None;

        return st;
    }

    public bool TryUnlockSkill(SkillType skillType) {
        if (skillType != SkillType.None && !IsSkillUnlocked(skillType) && CanUnlock(skillType)) {
            if (skillPoints > 0) {
                skillPoints--;
                OnSkillPointsChanged?.Invoke(this, EventArgs.Empty);
                UnlockSkill(skillType);
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

}
