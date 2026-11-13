using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ResourceNode : Interactable
{
    public ResourceNodeDefinition definition;
    public GameObject availableVisual, depletedVisual;
    public AudioSource audioSource;
    public UnityEvent onCollected = new UnityEvent(), onRespawned = new UnityEvent();
    public event Action<Transform, Item, int> ResourceCollected;
    private int charges;
    private float respawnAt;
    private bool initialized;
    private PlayerGathering collector;
    private Coroutine respawnRoutine;
    public int Charges { get { Refresh(); return charges; } }
    public bool IsDepleted => Charges == 0;
    public bool IsGathering => collector != null;
    public float RespawnRemaining => IsDepleted ? Mathf.Max(0, respawnAt - Time.time) : 0;
    public string Prompt => definition == null ? "Unconfigured resource" : IsDepleted ? definition.displayName + " (depleted)" : IsGathering ? "In use" : "[E] " + definition.actionLabel + " " + definition.displayName;
    private void OnEnable() { Refresh(); ScheduleRespawn(); }
    private void OnDisable()
    {
        if (collector != null) collector.Cancel("Gathering interrupted.");
        if (respawnRoutine != null) StopCoroutine(respawnRoutine);
        respawnRoutine = null;
    }
    private void Refresh()
    {
        if (!initialized && definition != null) { charges = Mathf.Max(1, definition.maxCharges); initialized = true; ApplyVisual(); }
        if (initialized && charges == 0 && Time.time >= respawnAt)
        {
            charges = Mathf.Max(1, definition.maxCharges); ApplyVisual(); onRespawned.Invoke();
        }
    }
    private void ApplyVisual()
    {
        if (availableVisual != null && availableVisual != gameObject) availableVisual.SetActive(charges > 0);
        if (depletedVisual != null && depletedVisual != gameObject) depletedVisual.SetActive(charges == 0);
    }
    public override void Interact(Transform owner)
    {
        var gathering = owner.GetComponent<PlayerGathering>();
        if (gathering != null) gathering.TryBegin(this);
    }
    public bool TryReserve(PlayerGathering owner)
    {
        Refresh();
        if (owner == null || !isActiveAndEnabled || definition == null || !definition.IsValid || charges <= 0 || collector != null) return false;
        collector = owner; return true;
    }
    public void Release(PlayerGathering owner) { if (collector == owner) collector = null; }
    public bool Commit(PlayerGathering owner, Transform player, Inventory inventory)
    {
        if (owner == null || owner.ActiveNode != this || collector != owner || !isActiveAndEnabled || charges <= 0 || definition == null || !definition.IsValid) return false;
        if (!definition.RequirementsMet(player, inventory, out _)) return false;
        if (!inventory.TryExchange(Array.Empty<InventoryItemAmount>(), definition.Rewards, false)) return false;
        // State commits before observers can reenter interaction.
        collector = null; charges--;
        if (charges == 0) { respawnAt = Time.time + Mathf.Max(.01f, definition.respawnDuration); ApplyVisual(); ScheduleRespawn(); }
        inventory.NotifyExchange();
        if (audioSource != null && definition.collectionSound != null) audioSource.PlayOneShot(definition.collectionSound);
        ResourceCollected?.Invoke(player, definition.resourceItem, definition.quantity);
        onCollected.Invoke(); return true;
    }
    private void ScheduleRespawn()
    {
        if (!isActiveAndEnabled || !initialized || charges > 0 || respawnRoutine != null) return;
        respawnRoutine = StartCoroutine(Respawn());
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(Mathf.Max(.01f, respawnAt - Time.time));
        respawnRoutine = null; Refresh();
    }
    [Serializable] public struct Snapshot { public int charges; public float remainingSeconds; }
    public Snapshot CaptureState() => new Snapshot { charges = Charges, remainingSeconds = RespawnRemaining };
    public void RestoreState(Snapshot state)
    {
        if (collector != null) collector.Cancel("Resource state changed.");
        if (respawnRoutine != null) StopCoroutine(respawnRoutine);
        respawnRoutine = null; initialized = definition != null;
        charges = definition == null ? 0 : Mathf.Clamp(state.charges, 0, Mathf.Max(1, definition.maxCharges));
        respawnAt = Time.time + (float.IsNaN(state.remainingSeconds) || float.IsInfinity(state.remainingSeconds) ? 0 : Mathf.Max(0, state.remainingSeconds));
        ApplyVisual(); Refresh(); ScheduleRespawn();
    }
}
