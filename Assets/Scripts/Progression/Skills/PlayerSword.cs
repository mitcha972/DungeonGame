using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public static PlayerSword instance;
    public UI_SkillTree UITree;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        switch (UI_SkillTree.selectedSkill)
        {
            case PlayerSkills.SkillType.MoveSpeed_1:
            case PlayerSkills.SkillType.MoveSpeed_2:
            case PlayerSkills.SkillType.HealthMax_1:
            case PlayerSkills.SkillType.HealthMax_2:
                Debug.Log("Used Skill " + UI_SkillTree.selectedSkill);
                break;
        }
    }
}
