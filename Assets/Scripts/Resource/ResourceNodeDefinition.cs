using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Resources/Resource Node Definition")]
public class ResourceNodeDefinition : ScriptableObject
{
    public string displayName = "Resource", actionLabel = "Collect";
    public Item resourceItem;
    [Min(1)] public int quantity = 1;
    [Min(.1f)] public float collectionDuration = 2;
    public string animationState = "GatheringCollect";
    [Min(.01f)] public float respawnDuration = 60;
    [Min(1)] public int maxCharges = 1;
    [Tooltip("Must be in the bag; not consumed. Use a custom requirement for equipped tools.")]
    public Item requiredItem;
    public ResourceNodeRequirement[] requirements = Array.Empty<ResourceNodeRequirement>();
    public AudioClip collectionSound;
    public bool IsValid => resourceItem != null && quantity > 0 && collectionDuration > 0 && !float.IsInfinity(collectionDuration) && respawnDuration > 0 && !float.IsInfinity(respawnDuration) && maxCharges > 0;
    public InventoryItemAmount[] Rewards => new[] { new InventoryItemAmount { item = resourceItem, count = quantity } };
    public bool RequirementsMet(Transform player, Inventory inventory, out string reason)
    {
        reason = "You need the required tool or item.";
        if (requiredItem != null && inventory.Count(requiredItem) == 0) return false;
        if (requirements != null) foreach (var requirement in requirements)
            if (requirement != null && !requirement.Evaluate(player, out reason)) return false;
        reason = ""; return true;
    }
}
