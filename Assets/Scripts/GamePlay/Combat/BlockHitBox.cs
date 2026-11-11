using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHitBox : MonoBehaviour
{
    CharacterStats opponentStats;
    CharacterStats myStats;
    CharacterCombat player;
    GameObject owner;

    //Shiled Object
    BoxCollider boxCollider;
    MeshRenderer meshRenderer;

    public float durability;

    public bool isBlocking;
    public bool canBlock;

    public List<Projectile> DHB = new List<Projectile>();

    void Awake()
    {
        canBlock = true;
        player = GetComponentInParent<CharacterCombat>();
        myStats = GetComponentInParent<CharacterStats>();
        owner = player != null ? player.gameObject : transform.root.gameObject;

        // Get the BoxCollider and MeshRenderer components from the current object
        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        durability = 100;
    }

    void Update()
    {
        DHB.RemoveAll(projectile => projectile == null);
        // Durability mechanics remain unchanged; combat owns damage reduction.
        CheckBlock();
    }

    private void OnDisable()
    {
        DisableBlock();
        RemoveCollisonList();
    }

    public void EnableBlock()
    {        
        if (!canBlock || boxCollider == null || meshRenderer == null)
            return;

        Debug.Log("Block Enabled");
        // Enable the MeshRenderer and BoxCollider components
        meshRenderer.enabled = true;
        boxCollider.enabled = true;

        //Set blocking to true
        isBlocking = true;
    }

    public void DisableBlock()
    {
        if (!isBlocking)
            return;

        // Enable the MeshRenderer and BoxCollider components
        if (meshRenderer != null) meshRenderer.enabled = false;
        if (boxCollider != null) boxCollider.enabled = false;

        //Set blocking to true
        isBlocking = false;

        //remove collsion
        RemoveCollisonList();
    }

    void CheckBlock()
    {
        //If blocking is not active the 
        if(!isBlocking)
            if(durability <=0)
                durability += 1f * Time.deltaTime;        
    }

    //Check For collison with enemy
    void OnTriggerEnter(Collider collision)
    {   
        if(collision.gameObject == owner || isBlocking == false) 
            return;

        //if collide with projectice
        if(collision.gameObject.GetComponent<Projectile>())
           Hit(collision);         
    }    

    void Hit(Collider collision)
    { 
        Projectile dhb = collision.gameObject.GetComponent<Projectile>();
        Debug.Log("Sheild Hit" + collision);
        
        if (dhb == null || dhb.owner == owner || DHB.Contains(dhb)) return;
        DHB.Add(dhb);

        //LowerSheild duriblitlty
        //CalculateDurability(dhb.damage);

        //Hit Effect
        //

    }

    void CalculateDurability(int DamageBlocked)
    {
        // Calculate dur
        durability -= DamageBlocked;

        if(durability <= 0)
            BreakBlock();
    }

    public void RemoveCollisonList()
    {
        DHB.Clear();
    }

    //If the block duriblity is too low
    void BreakBlock()
    {
        //diable sheild Obj
        DisableBlock();       

        //Block Break Animation;
        //


    }

    
}
