using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Info")]
    [HideInInspector] public CharacterStats myStats;
    [HideInInspector] public CharacterCombat target;
    [HideInInspector] public GameObject owner;
    public WeaponClass weaponClass;

    [Header("Stats")]
    private int damage;
    public GameObject collisionEffect;
    private float range;
    [HideInInspector] public float speed;

    [Header("Bools")]
    public bool rangeEnd;
    public bool destroyOnCollide;
    
    private LayerMask collisionMask;
    public LayerMask solidCollisionMask;

    private Vector3 startPosition;
    private List<CharacterStats> collidedCharacters; 

    private void Awake()
    {
        //attackRange = weaponClass.range;
        //rangeEnd = weaponClass.rangeEnd;
        //destroyOnCollide = weaponClass.destroyOnCollide;
        collisionMask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Wall");
        startPosition = transform.position;
        collidedCharacters = new List<CharacterStats>();
    }

    public void StartUp(CharacterStats MyStats, GameObject Owner, int aDamage, float aRange, float aSpeed)
    {
        myStats = MyStats;
		owner = Owner;
        damage = aDamage;
        range = aRange;
        speed = aSpeed;
    }

    private void Update()
    {      
        CheckRange();
    }
    
    private void CheckRange()
    {
        if(rangeEnd)    
        {
            float dist = Vector3.Distance(startPosition, transform.position);
            if(dist >= range)
            {
                Destroy(gameObject); 
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {  
        //If has Same tag
        if(other.CompareTag(this.tag))
            return;
        
        // int otherLayer = 1 << other.gameObject.layer;
        // if((collisionMask & otherLayer) == otherLayer)
        //     return;

        if ((solidCollisionMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Debug.Log("HitWalllll");
            CollisionEffect(); 
            DestroyObject();
            return;
        }

        if(other.TryGetComponent(out DestructibleObject destructibleObject))
        { 
            destructibleObject.Damage(damage, transform.position);
            DestroyObject();
            return;
        }

        if(other.TryGetComponent(out CharacterStats characterStats))
        {   
            if(!collidedCharacters.Contains(characterStats))
            {
                collidedCharacters.Add(characterStats);
                Debug.Log("Hit Player");
                CharacterCollision(other.gameObject);
            }
        }    
    }  

    private void CharacterCollision(GameObject other)
    {
        //Target Info
        target = other.GetComponent<CharacterCombat>();
        Vector3 direction = transform.position - target.transform.position;
        Vector3 planeNormal = target.transform.up;
        Vector3 projectedDirection = Vector3.ProjectOnPlane(direction, planeNormal);
        float angle = Vector3.SignedAngle(projectedDirection, target.transform.forward, planeNormal);

        //Check Block
        bool isBlocked = CheckBlock(other);
        int damageReduction = 0;

        if(isBlocked)
        {
            Debug.Log("Attack blockedddd");
            damageReduction += other.GetComponent<CharacterStats>().armor;
            //damageReduction += 100;
        }
        else
            damageReduction += 0;

        //Deal Damage And destroy
        DealDamage(damageReduction, angle, isBlocked);
        CollisionEffect(); 
        DestroyObject();
    }

    private bool CheckBlock(GameObject other)
    {

        BlockHitBox blockHitBox = other.GetComponent<CharacterCombat>().blockHitBox;

        if(blockHitBox)
        {
            for(int i = 0 ; i < blockHitBox.DHB.Count; i++)
            {
                if(blockHitBox.DHB[i] == this)
                {
                    blockHitBox.DHB[i] = null;
                    return true;
                }
            }
        }
        return false;      
    }

    void CollisionEffect()
    {
         // Instantiate the collision effect at the position of the collision
        GameObject effect = Instantiate(collisionEffect, transform.position, Quaternion.identity);
        
        // Destroy the effect after a certain amount of time
        Destroy(effect, 1.0f);
    }

    private void DealDamage(int damageReduction, float angle, bool isBlocked)
    {
        target.TakeDamage(damage - damageReduction, owner.transform, angle, transform.position, isBlocked);
    }
    
    private void DestroyObject()
    {  
        if(destroyOnCollide)
            Destroy(gameObject);
    }
}
