using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class AttackMotion
{
    public AnimationClip clip;
    [SerializeField, HideInInspector] private string animatorState;
    public string AnimatorState => animatorState;
}

/// <summary>Authored attacks and delivery settings referenced directly by an equipment item.</summary>
[CreateAssetMenu(fileName = "Weapon Attacks", menuName = "Combat/Weapon Attack Set")]
public class WeaponAttackSet : ScriptableObject
{
    public AttackMotion[] lightAttacks = Array.Empty<AttackMotion>();
    public AttackMotion[] heavyAttacks = Array.Empty<AttackMotion>();
    [FormerlySerializedAs("hitStart"), Range(0f, 1f)] public float damageWindowStart = .3f;
    [FormerlySerializedAs("hitEnd"), Range(0f, 1f)] public float damageWindowEnd = .6f;
    [FormerlySerializedAs("cooldown"), Min(.1f)] public float attackInterval = .8f;
    [Min(0f)] public float heavyDamageMultiplier = 1.5f;
    [Header("Projectile delivery")]
    [FormerlySerializedAs("isRanged")] public bool launchesProjectile;
    [FormerlySerializedAs("projectile")] public GameObject projectilePrefab;
    [Min(1f)] public float projectileSpeed = 25f;
    [Min(1f)] public float projectileRange = 40f;
    public AttackMotion SelectMotion(bool heavy)
    {
        var motions = heavy ? heavyAttacks : lightAttacks;
        if (motions == null) return null;
        int valid = 0;
        foreach (var motion in motions) if (motion != null && motion.clip != null) valid++;
        if (valid == 0) return null;
        int selection = UnityEngine.Random.Range(0, valid);
        foreach (var motion in motions)
            if (motion != null && motion.clip != null && selection-- == 0) return motion;
        return null;
    }
}
