using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    public CharacterStats myStats;
    public CharacterCombat target;
    public GameObject owner;
    BoxCollider boxCollider;
    private LayerMask collisionMask;
    public List<CharacterStats> collidedCharacters;
    
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        collisionMask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Wall");
        collidedCharacters = new List<CharacterStats>();
    }

    public void UpdateAttackHitBox(Equipment equipment)//remove
    {
        //boxCollider.size = new Vector3(equipment.xLength, equipment.yLength, equipment.zLength);
    }

    private void DealDamage(int damageReduction, float angle , bool isBlocked)
    {
        target.TakeDamage(myStats.damage - damageReduction, owner.transform, angle, transform.position, isBlocked);
    }

    public void EnableDamage()
    {
       
        boxCollider.enabled = true;
    }

    public void DisableDamage()
    {
        //RestSet Attack
        target = null; 
        boxCollider.enabled = false;
        collidedCharacters.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {  
        //If same tage
        if(other.CompareTag(this.tag))
            return;
        
        int otherLayer = 1 << other.gameObject.layer;
        if((collisionMask & otherLayer) == otherLayer)
            return;
        
        Debug.LogWarning("Trigger Hit " + other);
        if(other.TryGetComponent(out DestructibleObject destructibleObject))
        { 
            destructibleObject.Damage(myStats.damage, transform.position);
            return;
        }

        if(other.TryGetComponent(out CharacterStats characterStats))
        {   
            if(!collidedCharacters.Contains(characterStats))
            {
                collidedCharacters.Add(characterStats);
                //Debug.Log("Hit Player");
                CharacterCollision(other.gameObject);
            }
        }    
    }  

    private void CharacterCollision(GameObject other)
    {
        target = other.GetComponent<CharacterCombat>();
        Vector3 direction = transform.position - target.transform.position;
        Vector3 planeNormal = target.transform.up;
        Vector3 projectedDirection = Vector3.ProjectOnPlane(direction, planeNormal);
        float angle = Vector3.SignedAngle(projectedDirection, target.transform.forward, planeNormal);

        //Fix
        bool isBlocked = CheckBlock(other);
        int damageReduction = 0;

        if(isBlocked)
        {
            //Debug.Log("Attack blockedddd");
            damageReduction += other.GetComponent<CharacterStats>().armor;
        }
        else
            damageReduction += 0;

        DealDamage(damageReduction, angle , isBlocked);
    }

    private bool CheckBlock(GameObject other)
    {
        BlockHitBox blockHitBox = other.GetComponent<CharacterCombat>().blockHitBox;
        
        if(other.GetComponent<CharacterCombat>().isBlocking)
        {   
            //Debug.Log("WE SAYYY Attack blockedddd");
            return true;
        }
        
        /*
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
        */
        return false;      
    }

}
