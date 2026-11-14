using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectApplyResult { Applied, AlreadyActive, InvalidEffect, InvalidTarget }

public sealed class ActiveStatusEffect
{
    public StatusEffectDefinition Definition { get; }
    public string Id { get; }
    public Transform Source { get; }
    public float RemainingDuration => (float)Math.Max(0, duration - elapsed);
    internal readonly float duration, interval, movement, outgoing, incoming;
    internal readonly int damage;
    internal double elapsed, nextTick;
    internal ActiveStatusEffect(StatusEffectDefinition definition, Transform source)
    {
        Definition = definition; Id = definition.effectId; Source = source;
        duration = definition.duration; interval = definition.tickInterval; damage = definition.damagePerTick;
        movement = Mathf.Clamp(definition.movementMultiplier, 0, 10);
        outgoing = Mathf.Clamp(definition.outgoingDamageMultiplier, 0, 10);
        incoming = Mathf.Clamp(definition.incomingDamageMultiplier, 0, 10);
        nextTick = interval;
    }
}

[DisallowMultipleComponent]
public sealed class StatusEffectController : MonoBehaviour
{
    private readonly List<ActiveStatusEffect> active = new List<ActiveStatusEffect>();
    private IReadOnlyList<ActiveStatusEffect> view;
    private CharacterStats stats;
    private bool clearing;
    public IReadOnlyList<ActiveStatusEffect> ActiveEffects => view ?? (view = active.AsReadOnly());
    public float MovementMultiplier { get; private set; } = 1;
    public float OutgoingDamageMultiplier { get; private set; } = 1;
    public float IncomingDamageMultiplier { get; private set; } = 1;
    public event Action<ActiveStatusEffect> EffectApplied;
    public event Action<ActiveStatusEffect> EffectRemoved;
    public event Action ActiveEffectsChanged;
    private void Awake() { stats = GetComponent<CharacterStats>(); }
    private void OnDisable() { ClearAllEffects(); }
    private void Update() { Advance(Time.deltaTime); }
    public StatusEffectApplyResult TryApplyEffect(StatusEffectDefinition effect, Transform source = null)
    {
        if (effect == null || !effect.IsValid) return StatusEffectApplyResult.InvalidEffect;
        if (clearing || !isActiveAndEnabled || stats == null || !stats.isActiveAndEnabled || stats.IsDead || stats.currentHealth <= 0)
            return StatusEffectApplyResult.InvalidTarget;
        if (HasEffect(effect.effectId)) return StatusEffectApplyResult.AlreadyActive;
        var instance = new ActiveStatusEffect(effect, source);
        active.Add(instance); Recalculate();
        EffectApplied?.Invoke(instance); ActiveEffectsChanged?.Invoke();
        return StatusEffectApplyResult.Applied;
    }
    public bool HasEffect(string id) => Find(id) != null;
    public float GetRemainingDuration(string id) => Find(id)?.RemainingDuration ?? 0;
    private ActiveStatusEffect Find(string id) { foreach (var effect in active) if (effect.Id == id) return effect; return null; }
    public bool RemoveEffect(string id)
    {
        var effect = Find(id); if (effect == null) return false;
        active.Remove(effect); Recalculate(); EffectRemoved?.Invoke(effect); ActiveEffectsChanged?.Invoke(); return true;
    }
    public void ClearAllEffects()
    {
        if (clearing) return;
        clearing = true;
        try { while (active.Count > 0) RemoveEffect(active[active.Count - 1].Id); }
        finally { clearing = false; }
    }
    private void Recalculate()
    {
        double movement = 1, outgoing = 1, incoming = 1;
        foreach (var effect in active)
        {
            movement *= effect.movement; outgoing *= effect.outgoing; incoming *= effect.incoming;
        }
        MovementMultiplier = (float)Math.Min(10, movement);
        OutgoingDamageMultiplier = (float)Math.Min(10, outgoing);
        IncomingDamageMultiplier = (float)Math.Min(10, incoming);
    }
    // Scaled gameplay seconds; ticks at interval, 2*interval ... strictly BEFORE duration.
    // Catch up all scheduled ticks after long frames; never add a tick at expiration.
    internal void Advance(float delta)
    {
        if (stats == null || !stats.isActiveAndEnabled || stats.IsDead || stats.currentHealth <= 0) { ClearAllEffects(); return; }
        if (delta <= 0 || float.IsNaN(delta) || float.IsInfinity(delta)) return;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (i >= active.Count) continue;
            var effect = active[i]; effect.elapsed = Math.Min(effect.duration, effect.elapsed + delta);
            while (effect.damage > 0 && effect.nextTick <= effect.elapsed && effect.nextTick < effect.duration && active.Contains(effect))
            {
                effect.nextTick += effect.interval;
                stats.TakeDamage(effect.damage, effect.Source, 0);
                if (stats.IsDead || stats.currentHealth <= 0) { ClearAllEffects(); return; }
            }
            if (effect.elapsed >= effect.duration && active.Contains(effect)) RemoveEffect(effect.Id);
        }
    }
}
