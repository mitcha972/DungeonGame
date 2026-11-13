using System;
using UnityEngine;

[Serializable] public sealed class SkillProgressSnapshot
{
    public int version = 1;
    public int[] experience = new int[10];
}

[DisallowMultipleComponent]
public sealed class PlayerSkillProgress : MonoBehaviour
{
    public SkillXpCurve curve;
    public SkillDefinition[] definitions = Array.Empty<SkillDefinition>();
    [Min(1)] public int damageXpMultiplier = 1;
    [Min(1)] public int successfulBlockXp = 10;
    [SerializeField] private int[] experience = new int[10];
    public event Action<RpgSkill, int, Vector3?> OnXpGained;
    public event Action<RpgSkill, int> OnSkillLevelUp;
    private bool notifying;
    public static bool Valid(RpgSkill skill) => (int)skill >= 0 && (int)skill < 10;
    public bool CanAward(RpgSkill skill, int amount) => isActiveAndEnabled && curve != null && Valid(skill) && amount > 0 && !notifying && GetXP(skill) < int.MaxValue;
    public int GetXP(RpgSkill skill) => Valid(skill) && experience != null && experience.Length == 10 ? experience[(int)skill] : 0;
    public int GetLevel(RpgSkill skill) => curve != null && Valid(skill) ? curve.Level(GetXP(skill)) : 1;
    public int XPToNextLevel(RpgSkill skill) => GetLevel(skill) >= 30 || curve == null ? 0 : curve.Threshold(GetLevel(skill) + 1) - GetXP(skill);
    public string SkillName(RpgSkill skill)
    {
        foreach (var definition in definitions) if (definition != null && definition.skill == skill && !string.IsNullOrEmpty(definition.displayName)) return definition.displayName;
        return skill.ToString();
    }
    private void Awake() { if (experience == null || experience.Length != 10) experience = new int[10]; }
    public bool AwardXP(RpgSkill skill, int amount, Vector3? worldPosition = null)
    {
        if (!CanAward(skill, amount)) return false;
        int oldLevel = GetLevel(skill), oldXP = GetXP(skill);
        experience[(int)skill] = (int)Math.Min(int.MaxValue, (long)oldXP + amount);
        int newLevel = GetLevel(skill);
        notifying = true;
        try
        {
            Notify(OnXpGained, handler => handler(skill, GetXP(skill) - oldXP, worldPosition));
            for (int level = oldLevel + 1; level <= newLevel; level++)
            { int reached = level; Notify(OnSkillLevelUp, handler => handler(skill, reached)); }
        }
        finally { notifying = false; }
        return true;
    }
    private static void Notify<T>(T listeners, Action<T> invoke) where T : Delegate
    {
        if (listeners == null) return;
        foreach (var listener in listeners.GetInvocationList())
            try { invoke((T)listener); } catch (Exception ex) { Debug.LogException(ex); }
    }
    public SkillProgressSnapshot CaptureState() => new SkillProgressSnapshot { experience = (int[])experience.Clone() };
    public bool RestoreState(SkillProgressSnapshot snapshot)
    {
        if (notifying || snapshot == null || snapshot.version != 1 || snapshot.experience == null || snapshot.experience.Length != 10) return false;
        foreach (int xp in snapshot.experience) if (xp < 0) return false;
        experience = (int[])snapshot.experience.Clone(); return true;
    }
    public void AwardDamage(CharacterStats victim, int healthLost, bool ranged, Vector3 position)
    {
        var player = GetComponent<PlayerStats>();
        if (player == null || player.IsDead || player.currentHealth <= 0 || victim == null || !(victim is EnemyStats) || !victim.enabled || victim.CompareTag(gameObject.tag) || healthLost <= 0) return;
        AwardXP(ranged ? RpgSkill.Archery : RpgSkill.Combat, (int)Math.Min(int.MaxValue, (long)healthLost * Mathf.Max(1, damageXpMultiplier)), position);
    }
}
