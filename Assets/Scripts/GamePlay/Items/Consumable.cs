using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatModifier { Health, Damage }

[CreateAssetMenu(fileName = "New Consumable", menuName = "Item/Consumable")]
public class Consumable : Item
{
    [Header("Consumable")]
    public int StatAmount;
   
    public StatModifier statModifier; // Declare statModifier variable
    public GameObject particleEffect;

    public override void Use(GameObject owner, int SlotIndex)
    {
        //Play Animation
        owner.GetComponent<CharacterAnimator>().PlayTargetAnimation("Drink", true);

        Debug.Log("Used " + this.name);
        PlayerStats PS = owner.GetComponent<PlayerStats>();

        // Check modifier
        switch (statModifier)
        {
            case StatModifier.Health:
                PS.Heal(StatAmount);
                break;

            case StatModifier.Damage:
                // TODO: Handle damage modification
                break;
        }

        //Particle Effect
        if(particleEffect)
        {
            // Set the offset to 0.2 units above the player's position
            Vector3 offset = new Vector3(0.0f, 0.2f, 0.0f);

            // Instantiate particle effect at owner's position with owner as parent and original rotation
            GameObject effect = Instantiate(particleEffect, owner.transform.position + offset, particleEffect.transform.rotation, owner.transform);
            Destroy(effect, 3.0f);

        }      

    }
}
