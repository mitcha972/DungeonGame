using UnityEngine;

/// <summary>Future tool, skill and quest requirements share this check.</summary>
public abstract class ResourceNodeRequirement : ScriptableObject
{
    public abstract bool Evaluate(Transform player, out string reason);
}
