using UnityEngine;

public enum Target {Self, Cast}

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class Abilities : ScriptableObject
{
    new public string name = "New Ability";	
	new public string description = "Ability Description";
    
    [Header("Ability")]
    public Target target;
    public GameObject particleEffect;
    public float cooldownTime;
    public float modifier; 
    StatModifier statModifier;
   
    [Header("Projectile")]
    public GameObject projectile;
    public int damage;
    public float range;
    public float speed;

    [Header("Animation")]
    public AnimationClip startAnimation;
    public AnimationClip castAnimation;
}
