using UnityEngine;

/// <summary>Owns animation presentation and weapon animation selection, not damage execution.</summary>
public class CharacterAnimator : MonoBehaviour
{
    public WeaponClass defaultWeaponClass;
    public WeaponClass[] availableWeaponClasses;
    public AnimationClip replaceableLightAttackAnim;
    public AnimationClip replaceableHeavyAttackAnim;
    public AnimationClip replaceableAbilityAnim;
    public AnimatorOverrideController overrideController;
    protected Animator animator;
    protected CharacterCombat combat;
    private EquipmentManager _equipment;
    public WeaponClass CurrentWeapon { get; private set; }
    public float CurrentAttackDuration { get; private set; }
    public event System.Action EnableMovement;
    public event System.Action DisableMovement;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        combat = GetComponent<CharacterCombat>();
        _equipment = GetComponentInChildren<EquipmentManager>();
        CurrentWeapon = defaultWeaponClass;
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("CharacterAnimator requires an Animator with a controller.", this);
            enabled = false;
            return;
        }
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
        if (_equipment != null) _equipment.onEquipmentChanged += OnEquipmentChanged;
        if (combat != null) combat.OnAttack += OnAttack;
    }
    private void OnDestroy()
    {
        if (_equipment != null) _equipment.onEquipmentChanged -= OnEquipmentChanged;
        if (combat != null) combat.OnAttack -= OnAttack;
        if (overrideController != null) Destroy(overrideController);
    }
    public bool AnimationIsBusy() => animator != null && animator.GetBool("isBusy");
    public void PlayTargetAnimation(string targetAnim, bool makeBusy)
    {
        if (animator == null) return;
        int stateHash = Animator.StringToHash(targetAnim);
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            if (!animator.HasState(layer, stateHash)) continue;
            animator.SetBool("isBusy", makeBusy);
            if (layer > 0) animator.SetLayerWeight(layer, 1f);
            animator.CrossFade(stateHash, 0.1f, layer);
            return;
        }
        Debug.LogWarning("Animator state is missing: " + targetAnim, this);
    }
    public void SetBusy(bool busy)
    {
        if (animator != null) animator.SetBool("isBusy", busy);
        if (busy) DisableMovement?.Invoke(); else EnableMovement?.Invoke();
    }
    public void PlayAbilityAnimation(AnimationClip clip)
    {
        if (clip == null || overrideController == null) return;
        if (replaceableAbilityAnim != null) overrideController[replaceableAbilityAnim] = clip;
        PlayTargetAnimation("Ability", true);
    }
    private void OnAttack(bool heavy)
    {
        if (overrideController == null) return;
        var weapon = CurrentWeapon != null ? CurrentWeapon : defaultWeaponClass;
        CurrentAttackDuration = weapon != null ? weapon.attackDuration : 0.8f;
        var clips = weapon == null ? null : heavy ? weapon.heavyAttackClips : weapon.lightAttackClips;
        var placeholder = heavy ? replaceableHeavyAttackAnim : replaceableLightAttackAnim;
        if (clips != null && clips.Length > 0 && placeholder != null)
        {
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null) { overrideController[placeholder] = clip; CurrentAttackDuration = Mathf.Max(0.1f, clip.length); }
        }
        PlayTargetAnimation(heavy ? "HeavyAttack" : "LightAttack", true);
    }
    public void GetHit(float direction)
    {
        if (animator == null) return;
        animator.SetFloat("HitAngle", direction);
        PlayTargetAnimation("GetHit", true);
    }
    public void OnEquipmentChanged(Equipment newItem, Equipment oldItem)
    {
        if (animator == null) return;
        if (newItem != null && newItem.equipSlot == EquipmentSlot.MainHand)
        {
            CurrentWeapon = defaultWeaponClass;
            if (availableWeaponClasses != null)
                foreach (var candidate in availableWeaponClasses)
                    if (candidate != null && candidate.itemType == newItem.itemType) { CurrentWeapon = candidate; break; }
            SetEquipmentLayer("Right Hand", 1f);
        }
        else if (newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.MainHand)
        {
            CurrentWeapon = defaultWeaponClass;
            SetEquipmentLayer("Right Hand", 0f);
        }
        if ((newItem != null && newItem.equipSlot == EquipmentSlot.OffHand) || (oldItem != null && oldItem.equipSlot == EquipmentSlot.OffHand))
            SetEquipmentLayer("Left Hand", newItem != null ? 1f : 0f);
        if (combat != null) combat.currentProjectile = CurrentWeapon != null && CurrentWeapon.isRanged ? CurrentWeapon.projectile : null;
    }
    private void SetEquipmentLayer(string name, float weight)
    {
        // Player and enemy controllers use different capitalization; indices are not a contract.
        for (int i = 1; i < animator.layerCount; i++)
            if (string.Equals(animator.GetLayerName(i), name, System.StringComparison.OrdinalIgnoreCase))
                animator.SetLayerWeight(i, weight);
    }
    public void Equip() { }
    private int gatheringHash, previousState;
    private float[] gatheringLayerWeights;
    public void BeginGathering(string state)
    {
        if (animator == null || string.IsNullOrEmpty(state)) return;
        gatheringHash = Animator.StringToHash(state);
        if (!animator.HasState(0, gatheringHash)) { gatheringHash = 0; return; }
        previousState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        gatheringLayerWeights = new float[animator.layerCount];
        for (int i = 1; i < animator.layerCount; i++) { gatheringLayerWeights[i] = animator.GetLayerWeight(i); animator.SetLayerWeight(i, 0); }
        SetBusy(true);
        animator.CrossFade(gatheringHash, .1f, 0);
    }
    public void EndGathering(bool interrupted)
    {
        if (animator == null || gatheringHash == 0) return;
        var current = animator.GetCurrentAnimatorStateInfo(0);
        bool stillGathering = current.shortNameHash == gatheringHash || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == gatheringHash);
        bool transitioningAway = animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash != gatheringHash;
        if (gatheringLayerWeights != null)
            for (int i = 1; i < animator.layerCount; i++) animator.SetLayerWeight(i, gatheringLayerWeights[i]);
        if (stillGathering && (!interrupted || !transitioningAway))
        {
            animator.CrossFade(previousState, .1f, 0);
            SetBusy(false);
        }
        else if (!interrupted) SetBusy(false);
        gatheringHash = 0; gatheringLayerWeights = null;
    }
    public void Death() => PlayTargetAnimation("Death", true);
    public void AttackHitEvent() { }
    [System.Serializable]
    public struct WeaponAnimations { public Equipment weapon; public AnimationClip[] clips; public GameObject projectile; }
}
