using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimator : MonoBehaviour 
{
    //Currnt Animtaion set being used // Change to Attack ANimation Class, (Basic Attacks, Heavy Attacks, Combo Attack)
    private AnimationClip[] lightAttackAnimSet;//change to light attack
    private AnimationClip[] heavyAttackAnimSet;

    //public AnimationClip[] defaultAttackAnimSet;//??
    
    public WeaponClass defaultWeaponClass;
    public WeaponClass[] availableWeaponClasses;

    //The Animation clip that can be replaced (Punch)
    public AnimationClip replaceableLightAttackAnim;//change to auto find // light attack
    public AnimationClip replaceableHeavyAttackAnim;
    public AnimationClip replaceableAbilityAnim;
    
    EquipmentManager equipmentManager; //make private or hide

    protected Animator animator;
    protected CharacterCombat combat;
    public AnimatorOverrideController overrideController;

    private Rigidbody rb;

    // This constant value determines the smoothness of the locomotion animation
    const float locomationAnimationSmoothTime = .1f;

    private bool canRotate;

    public event System.Action EnableMovement;
    public event System.Action DisableMovement;

    //Set Up A State Machine for Diffrent Aniatmino
        //Movement
        //Get Hit(can move false, can)

    void Awake () // Mmove to main 
	{
        equipmentManager = GetComponent<EquipmentManager>();
        animator = GetComponent<Animator>();
        combat = GetComponent<CharacterCombat>();

        rb =  GetComponent<Rigidbody>();

        //Animation Override
        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        }

        animator.runtimeAnimatorController = overrideController;

        //Set current animantion set to defaults
        lightAttackAnimSet = defaultWeaponClass.lightAttackClips;
        heavyAttackAnimSet = defaultWeaponClass.heavyAttackClips;

        // Does what?
        equipmentManager.onEquipmentChanged += OnEquipmentChanged;
        
        //when combat registers an attack, run animation attact script
        combat.OnAttack += OnAttack;
	}

    public bool AnimationIsBusy()
    {
        bool anim = animator.GetBool("isBusy"); 
        return anim;
    }

    public void PlayTargetAnimation(string targetAnim, bool makeBusy)
    {
        animator.SetBool("isBusy" , makeBusy);
        animator.CrossFade(targetAnim, 0.2f);
    }
    
    public void PlayAbilityAnimation(AnimationClip abilityAnim)
    {
        // Override the replaceable attack animation with the chosen attack animation
        overrideController[replaceableAbilityAnim.name] = abilityAnim;
        PlayTargetAnimation("Ability", true);
    }

    

    void OnAttack(bool isHeavyAttack)
    {
        //Check if is HeavOrLightAttack
        if(isHeavyAttack)
        {
            Debug.Log("HeavyATtakc");
            int attackIndex = Random.Range(0, heavyAttackAnimSet.Length);

            // // Override the replaceable attack animation with the chosen attack animation
            overrideController[replaceableHeavyAttackAnim.name] = heavyAttackAnimSet[attackIndex];
            PlayTargetAnimation("HeavyAttack", true);
        }
        else
        {
            Debug.Log("LightATtakc");
            // Choose a random attack animation from the current attack animation set
            int attackIndex = Random.Range(0, lightAttackAnimSet.Length);

            // Override the replaceable attack animation with the chosen attack animation
            overrideController[replaceableLightAttackAnim.name] = lightAttackAnimSet[attackIndex];
            PlayTargetAnimation("LightAttack", true);
        }

    }

    public void GetHit(float HitDir)
    {
        animator.SetFloat("HitAngle", HitDir);

        // Trigger the "GetHit" animation
        //animator.SetTrigger("GetHit");
        PlayTargetAnimation("GetHit", true);
        
        // Set the character's combat state to true
        combat.InCombat = true;

        // Update the "inCombat" parameter of the animator based on the character's combat state
        animator.SetBool("inCombat", combat.InCombat);
    }
  
    public void OnEquipmentChanged(Equipment newItem, Equipment oldItem) //called from 
    {
        //Change to State Machine //FIXXXX

        //If Remove a Weapon
        if (newItem == null)
        {
            //Remove Main Hand
            if (oldItem != null && oldItem.equipSlot == EquipmentSlot.MainHand)
            {
                //(newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.MainHand)
                animator.SetLayerWeight(1, 0);
                lightAttackAnimSet = defaultWeaponClass.lightAttackClips;
                heavyAttackAnimSet = defaultWeaponClass.heavyAttackClips;

                this.gameObject.GetComponent<CharacterCombat>().currentProjectile = defaultWeaponClass.projectile;
                
                return;
            }

            //Remove OffHand
            if (oldItem != null && oldItem.equipSlot == EquipmentSlot.OffHand)
            {
                animator.SetLayerWeight(2, 0);
                return;
            }

        }
        else
        {
            //Add Weapon 

            //If Main Hand 
            if (newItem.equipSlot == EquipmentSlot.MainHand) 
            {
                animator.SetLayerWeight(1, 1);
                
                //Check wepon list
                for(int i = 0; i< availableWeaponClasses.Length; i++)
                {      
                    if(availableWeaponClasses[i].itemType == newItem.itemType)
                    {
                        //Set Animation Clips
                        lightAttackAnimSet = availableWeaponClasses[i].lightAttackClips;
                        heavyAttackAnimSet = availableWeaponClasses[i].heavyAttackClips;
                        
                        //Move to combat (UpdateProjectile(currentProjectile))
                        this.gameObject.GetComponent<CharacterCombat>().currentProjectile = availableWeaponClasses[i].projectile;
                        //Debug.Log("current projectile type " + this.gameObject.GetComponent<CharacterCombat>().currentProjectile);
                    }
                }    

                return;                  
            }

            //Is Off Hand
            if (newItem.equipSlot == EquipmentSlot.OffHand)
            {
                animator.SetLayerWeight(2, 1);
                return;
            }
        }
    }

    public void Equip()
    {
        //animator.SetTrigger("Equip");
    }

    public void Death()
    {
        // Trigger the "Death" animation
        //animator.SetTrigger("Death");
        PlayTargetAnimation("Death", true);

        //...Does Something
    }

    /*
        ANIMATION EVENTS
        CALLED IN ANIMATOR
    */

    // public void EnableMovement()
    // {
    //     // TODO: Implement this method
    //     //movementController.canRotate = true;
    //     //FPC.CanRotate(true);
    // }

    // public void DisableMovemnt()
    // {
    //     // TODO: Implement this method
    //     //movementController.canRotate = false;
    //     //FPC.CanRotate(false);
    // }
    
    public void AttackHitEvent()
    {
        // This method is called when the character's attack animation activate event 

        //Debug.Log("Attack hit called");
        //combat.AttackHit_AnimationEvent();
    }

   
    [System.Serializable]
    public struct WeaponAnimations
    {
        public Equipment weapon;
        public AnimationClip[] clips;
        public GameObject projectile;
    }    
}
