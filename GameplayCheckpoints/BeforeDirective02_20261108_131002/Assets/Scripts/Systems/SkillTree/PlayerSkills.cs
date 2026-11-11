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

    public bool CanUnlock(SkillType skillType) {
        //SkillType skillRequirement = GetSkillRequirement(skillType);

        //array of neede skills
        int siz = GetSkillRequirement(skillType).Length;
        SkillType[] skillRequirement = new SkillType[siz];
        skillRequirement = GetSkillRequirement(skillType);
            
            
        //Object[] objects = new Object[]

        //For each skill requirement
        for(int i = 0; i < skillRequirement.Length; i++)
        {
        if (skillRequirement[i] != SkillType.None) {

            if (IsSkillUnlocked(skillRequirement[i])) {
                return true;
            } else {

                //if check all paths
                if(i == skillRequirement.Length - 1)
                    return false;
            }
        } else {
            //If no path is needed to unlock
            if(i <=0)
                return true;
        }
        
        }
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
        if (CanUnlock(skillType)) {
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
