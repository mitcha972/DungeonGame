using UnityEngine;

public class EnemyEquipmentManager : EquipmentManager
{
    // Retained only as a migration fallback for legacy prefabs without EnemyStats.
    [SerializeField, HideInInspector] private Enemy enemy;
    public override void Setup()
    {
        var stats = GetComponentInParent<EnemyStats>();
        var definition = stats != null ? stats.Definition : enemy;
        if (definition == null || definition.defaultEquipment == null) return;
        int equipLevel = GameController.floor * 5 + GameController.difficulty * 10 + Random.Range(-10, 11);
        foreach (var entry in definition.defaultEquipment)
        {
            if (entry == null || entry.equipment == null || entry.equipLevel > equipLevel) continue;
            if (Random.Range(0, 100) < Mathf.Clamp(entry.equipChance, 0, 100)) Equip(entry.equipment);
        }
    }
}
