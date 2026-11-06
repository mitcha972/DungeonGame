using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class DestructibleConObject : DestructibleObject
{
    //Public Item Spawn Chence [];

    public override void HealthSystem_OnDead(object sender, System.EventArgs e) 
    {
        base.HealthSystem_OnDead(sender, e);
        
        //Spawn Item
        //Transform wallBrokenTransform = Instantiate(pfObjectBroken, transform.position, transform.rotation);
               
    }
    
}