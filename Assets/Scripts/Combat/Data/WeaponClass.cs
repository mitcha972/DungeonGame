using UnityEngine;

[CreateAssetMenu(fileName = "New AttackClass", menuName = "Combat/AttackClass")]
public class WeaponClass : ScriptableObject//AttackClass
{
    //The Type of Item Needed to be Equipted to use this Attack
    public ItemType itemType;

    public LayerMask collisionMask;

    //Clips Used in the main attack
    public AnimationClip[] lightAttackClips;
    public AnimationClip[] heavyAttackClips;

    [Header("Attack Execution")]
    public bool isRanged;
    [Range(0f, 1f)] public float hitStart = 0.3f;
    [Range(0f, 1f)] public float hitEnd = 0.6f;
    [Min(0.1f)] public float attackDuration = 0.8f;
    [Min(0.1f)] public float cooldown = 0.8f;
    [Min(0f)] public float heavyDamageMultiplier = 1.5f;
    [Header("Projectile")]
    public GameObject projectile;
    [Min(1f)] public float projectileSpeed = 25f;
    [Min(1f)] public float projectileRange = 40f;
}

