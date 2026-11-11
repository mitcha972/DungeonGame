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
        if (owner == null || StatAmount <= 0 || statModifier != StatModifier.Health) return;
        var PS = owner.GetComponent<PlayerStats>();
        var inventory = owner.GetComponent<Inventory>();
        if (PS == null || PS.IsDead || PS.currentHealth >= PS.maxHealth || inventory == null || SlotIndex < 0 || SlotIndex >= inventory.items.Count || inventory.items[SlotIndex] != this) return;
        var animator = owner.GetComponent<CharacterAnimator>();
        if (animator != null) animator.PlayTargetAnimation("Drink", true);
        PS.Heal(StatAmount);
        inventory.Remove(SlotIndex);

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
