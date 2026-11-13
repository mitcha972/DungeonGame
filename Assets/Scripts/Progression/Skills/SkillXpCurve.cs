using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Skill XP Curve")]
public sealed class SkillXpCurve : ScriptableObject
{
    public const int MaxLevel = 30;
    [Min(1)] public int firstLevelCost = 100;
    [Min(1)] public int costIncrease = 50;
    public int Threshold(int level)
    {
        long n = Mathf.Clamp(level, 1, MaxLevel) - 1;
        // Bound authored costs so every supported threshold remains distinct in an int.
        long first = Mathf.Clamp(firstLevelCost, 1, 1000000);
        long step = Mathf.Clamp(costIncrease, 1, 1000000);
        return (int)(n * first + n * (n - 1) / 2 * step);
    }
    public int Level(int xp)
    {
        int level = 1;
        while (level < MaxLevel && xp >= Threshold(level + 1)) level++;
        return level;
    }
}
