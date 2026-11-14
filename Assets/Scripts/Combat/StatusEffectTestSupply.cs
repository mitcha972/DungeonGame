using UnityEngine;

// Explicit development fixture only; never bootstrap into production scenes.
public sealed class StatusEffectTestSupply : MonoBehaviour
{
    public Consumable poisonPotion;
    [Range(1, 5)] public int count = 3;
    private void Start()
    {
        var player = FindFirstObjectByType<PlayerStats>();
        var inventory = player != null ? player.GetComponent<Inventory>() : null;
        if (inventory != null) for (int i = 0; i < count; i++) inventory.Add(poisonPotion);
    }
}
