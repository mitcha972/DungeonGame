using UnityEngine;

public enum RpgSkill { Magic, Combat, Blocking, Archery, Crafting, Mining, Smithing, Cooking, Fishing, Wisdom }

[CreateAssetMenu(menuName = "Progression/Skill Definition")]
public sealed class SkillDefinition : ScriptableObject
{
    public RpgSkill skill;
    public string displayName;
    public Color color = new Color(1f, .8f, .35f);
}
