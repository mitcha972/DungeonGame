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

    [Header("Projectile")]
    public GameObject projectile; //if the AttackClass Has A projectile
}

