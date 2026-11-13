using System;
using UnityEngine;

[RequireComponent(typeof(PlayerControlState), typeof(Inventory))]
public class PlayerGathering : MonoBehaviour
{
    public ResourceNode ActiveNode { get; private set; }
    public bool IsGathering => ActiveNode != null;
    public float Progress => IsGathering ? Mathf.Clamp01((Time.time - startedAt) / duration) : 0;
    public string Feedback { get; private set; }
    public bool HasFeedback => Time.unscaledTime < feedbackUntil;
    public event Action<ResourceNode, Item, int> ResourceCollected;
    private PlayerControlState controls;
    private Inventory inventory;
    private PlayerStats stats;
    private PlayerController player;
    private CharacterAnimator animationOwner;
    private CharacterCombat combat;
    private float startedAt, duration, feedbackUntil;
    private int initialHealth;
    private bool ownsAction;
    private void Awake()
    {
        controls = GetComponent<PlayerControlState>(); inventory = GetComponent<Inventory>();
        stats = GetComponent<PlayerStats>(); player = GetComponent<PlayerController>();
        animationOwner = GetComponent<CharacterAnimator>(); combat = GetComponent<CharacterCombat>();
    }
    private void OnEnable() { controls.ActionCancelRequested += OnCancelRequested; }
    private void OnDisable() { controls.ActionCancelRequested -= OnCancelRequested; Cancel(""); }
    private void OnCancelRequested() => Cancel("You stop gathering.");
    public void ShowFeedback(string message) { Feedback = message; feedbackUntil = Time.unscaledTime + 3f; }
    public bool TryBegin(ResourceNode node)
    {
        if (ownsAction || !isActiveAndEnabled || !controls.GameplayAllowed || node == null) return false;
        if (node.definition == null || !node.definition.IsValid) { ShowFeedback("This resource is unavailable."); return false; }
        if (node.IsDepleted) { ShowFeedback("This resource is depleted."); return false; }
        if (!InRange(node)) return false;
        if (combat != null && (combat.IsExecutingAttack || combat.isBlocking)) { ShowFeedback("Finish your current action first."); return false; }
        if (animationOwner != null && animationOwner.AnimationIsBusy()) return false;
        if (!node.definition.RequirementsMet(transform, inventory, out string reason)) { ShowFeedback(reason); return false; }
        if (!inventory.CanExchange(Array.Empty<InventoryItemAmount>(), node.definition.Rewards)) { ShowFeedback("Your inventory is full."); return false; }
        if (!node.TryReserve(this)) { ShowFeedback("This resource is in use."); return false; }
        if (!controls.TryAcquireAction(this)) { node.Release(this); return false; }
        ownsAction = true; ActiveNode = node; startedAt = Time.time; duration = node.definition.collectionDuration;
        initialHealth = stats != null ? stats.currentHealth : 0;
        animationOwner?.BeginGathering(node.definition.animationState);
        ShowFeedback(node.definition.actionLabel + " " + node.definition.displayName + "..."); return true;
    }
    private bool InRange(ResourceNode node) => Vector3.Distance(transform.position, node.transform.position) <= (player != null ? player.InteractionDistance : 4f);
    private void Update()
    {
        if (!ownsAction) return;
        if (ActiveNode == null || !ActiveNode.isActiveAndEnabled || !controls.isActiveAndEnabled || (player != null && !player.isActiveAndEnabled) || !controls.ActionContextAllowed || !InRange(ActiveNode) || (stats != null && (stats.IsDead || stats.currentHealth <= 0 || stats.currentHealth < initialHealth)) || (combat != null && combat.IsExecutingAttack))
        { Cancel("Gathering interrupted."); return; }
        if (ActiveNode.definition == null || !ActiveNode.definition.IsValid) { Cancel("Resource is unavailable."); return; }
        if (!ActiveNode.definition.RequirementsMet(transform, inventory, out string reason)) { Cancel(reason); return; }
        if (Time.time - startedAt < duration) return;
        var node = ActiveNode;
        try
        {
            if (!node.Commit(this, transform, inventory)) { ShowFeedback("Your inventory cannot hold the reward."); return; }
            ShowFeedback("You collect " + node.definition.quantity + " x " + node.definition.resourceItem.name + ".");
            if (node.definition.awardsSkillXP)
                GetComponent<PlayerSkillProgress>()?.AwardXP(node.definition.xpSkill, node.definition.skillXP, node.transform.position);
            ResourceCollected?.Invoke(node, node.definition.resourceItem, node.definition.quantity);
        }
        finally { Finish(); }
    }
    public void Cancel(string reason) { if (!ownsAction) return; Finish(); if (!string.IsNullOrEmpty(reason)) ShowFeedback(reason); }
    private void Finish()
    {
        if (ActiveNode != null) ActiveNode.Release(this);
        ActiveNode = null; ownsAction = false;
        try { animationOwner?.EndGathering(stats != null && (stats.IsDead || stats.currentHealth < initialHealth)); }
        finally { controls.ReleaseAction(this); }
    }
}
