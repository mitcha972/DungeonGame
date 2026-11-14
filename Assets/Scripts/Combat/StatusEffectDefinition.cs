using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Status Effect")]
public sealed class StatusEffectDefinition : ScriptableObject
{
    public string effectId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    [Min(0.01f)] public float duration = 15;
    [Min(0.01f)] public float tickInterval = 2;
    [Min(0)] public int damagePerTick;
    [Range(0, 10)] public float movementMultiplier = 1;
    [Range(0, 10)] public float outgoingDamageMultiplier = 1;
    [Range(0, 10)] public float incomingDamageMultiplier = 1;
    public bool IsValid => !string.IsNullOrWhiteSpace(effectId) && Finite(duration) && duration > 0 &&
        Finite(tickInterval) && tickInterval > 0 && damagePerTick >= 0 &&
        Finite(movementMultiplier) && Finite(outgoingDamageMultiplier) && Finite(incomingDamageMultiplier);
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
