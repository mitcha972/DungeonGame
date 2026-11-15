using UnityEngine;

/// <summary>Plays authored character actions and equipment poses. Does not select combat rules or own equipment.</summary>
public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private string attackLayer = "Override";
    private Animator animator;
    private CharacterEquipment equipment;
    private int attackLayerIndex;
    private int requestedAttackState;
    private float[] previousLayerWeights;
    private bool[] transientLayers;
    private int[] requestedFrames;
    private static readonly int Busy = Animator.StringToHash("isBusy");
    public bool IsBusy => animator != null && animator.GetBool(Busy);
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        equipment = GetComponentInChildren<CharacterEquipment>(true);
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("CharacterAnimation requires an Animator with an authored controller.", this);
            enabled = false;
            return;
        }
        attackLayerIndex = animator.GetLayerIndex(attackLayer);
        previousLayerWeights = new float[animator.layerCount];
        transientLayers = new bool[animator.layerCount];
        requestedFrames = new int[animator.layerCount];
    }
    private void OnEnable() { if (equipment != null) equipment.EquipmentChanged += OnEquipmentChanged; }
    private void OnDisable() { if (equipment != null) equipment.EquipmentChanged -= OnEquipmentChanged; RestoreTransientLayers(); }
    private void Start() => RefreshEquipmentPoses();
    public bool CanPlayAttack(AttackMotion motion) => motion != null && motion.clip != null && !string.IsNullOrEmpty(motion.AnimatorState)
        && animator != null && attackLayerIndex >= 0 && animator.HasState(attackLayerIndex, Animator.StringToHash(motion.AnimatorState));
    private void ActivateActionLayer(int layer)
    {
        if (layer <= 0) return;
        if (layer == attackLayerIndex) { animator.SetLayerWeight(layer, 1f); return; }
        if (!transientLayers[layer]) previousLayerWeights[layer] = animator.GetLayerWeight(layer);
        transientLayers[layer] = true;
        requestedFrames[layer] = Time.frameCount;
        animator.SetLayerWeight(layer, 1f);
    }
    private void RestoreTransientLayers()
    {
        if (animator == null || transientLayers == null) return;
        for (int i = 1; i < transientLayers.Length; i++)
            if (transientLayers[i]) { animator.SetLayerWeight(i, previousLayerWeights[i]); transientLayers[i] = false; }
    }
    private void LateUpdate()
    {
        if (animator == null || transientLayers == null) return;
        for (int i = 1; i < transientLayers.Length; i++)
            if (transientLayers[i] && Time.frameCount > requestedFrames[i] && !animator.IsInTransition(i)
                && animator.GetCurrentAnimatorStateInfo(i).shortNameHash == Animator.StringToHash("Empty"))
            { animator.SetLayerWeight(i, previousLayerWeights[i]); transientLayers[i] = false; }
    }
    public void PlayAttack(AttackMotion motion)
    {
        if (!CanPlayAttack(motion)) return;
        RestoreTransientLayers();
        requestedAttackState = Animator.StringToHash(motion.AnimatorState);
        animator.SetLayerWeight(attackLayerIndex, 1f);
        animator.CrossFadeInFixedTime(Animator.StringToHash(motion.AnimatorState), .1f, attackLayerIndex, 0f);
    }
    public void CancelAttackMotion(AttackMotion motion)
    {
        if (motion == null || string.IsNullOrEmpty(motion.AnimatorState) || animator == null || attackLayerIndex < 0) return;
        int hash = Animator.StringToHash(motion.AnimatorState);
        if (requestedAttackState != hash) return;
        requestedAttackState = 0;
        animator.Play(Animator.StringToHash("Empty"), attackLayerIndex, 0f);
    }
    public bool PlayAbilityAnimation(Abilities ability)
    {
        if (ability == null || ability.startAnimation == null || string.IsNullOrEmpty(ability.AnimationState) || animator == null) return false;
        int hash = Animator.StringToHash(ability.AnimationState);
        int layer = -1;
        for (int i = 0; i < animator.layerCount; i++) if (animator.HasState(i, hash)) { layer = i; break; }
        if (layer < 0) return false;
        requestedAttackState = 0;
        ActivateActionLayer(layer);
        animator.CrossFadeInFixedTime(hash, .1f, layer, 0f);
        SetBusy(true);
        return true;
    }
    // Named, authored non-weapon actions: block, hit, equip, drink, death and gathering.
    public void PlayAction(string state, bool busy)
    {
        if (animator == null) return;
        requestedAttackState = 0;
        int hash = Animator.StringToHash(state);
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            if (!animator.HasState(layer, hash)) continue;
            animator.SetBool(Busy, busy);
            ActivateActionLayer(layer);
            animator.CrossFade(hash, .1f, layer);
            return;
        }
        Debug.LogWarning("Character action state is missing: " + state, this);
    }
    public void SetBusy(bool busy) { if (animator != null) animator.SetBool(Busy, busy); }
    public void GetHit(float direction)
    {
        if (animator == null) return;
        animator.SetFloat("HitAngle", direction);
        PlayAction("GetHit", true);
    }
    private void OnEquipmentChanged(Equipment item, Equipment previous) => RefreshEquipmentPoses();
    private void RefreshEquipmentPoses()
    {
        if (equipment == null || animator == null) return;
        SetPoseLayer("Right Hand", equipment.Items.ContainsKey(EquipmentSlot.MainHand));
        SetPoseLayer("Left Hand", equipment.Items.ContainsKey(EquipmentSlot.OffHand));
    }
    private void SetPoseLayer(string name, bool active)
    {
        for (int i = 1; i < animator.layerCount; i++)
            if (string.Equals(animator.GetLayerName(i), name, System.StringComparison.OrdinalIgnoreCase)) animator.SetLayerWeight(i, active ? 1f : 0f);
    }
    private int gatheringHash, previousState;
    private float[] gatheringLayerWeights;
    public void BeginGathering(string state)
    {
        if (animator == null || string.IsNullOrEmpty(state)) return;
        requestedAttackState = 0;
        RestoreTransientLayers();
        gatheringHash = Animator.StringToHash(state);
        if (!animator.HasState(0, gatheringHash)) { gatheringHash = 0; return; }
        previousState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        gatheringLayerWeights = new float[animator.layerCount];
        for (int i = 1; i < animator.layerCount; i++) { gatheringLayerWeights[i] = animator.GetLayerWeight(i); animator.SetLayerWeight(i, 0); }
        SetBusy(true); animator.CrossFade(gatheringHash, .1f, 0);
    }
    public void EndGathering(bool interrupted)
    {
        if (animator == null || gatheringHash == 0) return;
        var current = animator.GetCurrentAnimatorStateInfo(0);
        bool stillGathering = current.shortNameHash == gatheringHash || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == gatheringHash);
        bool leaving = animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash != gatheringHash;
        if (gatheringLayerWeights != null)
            for (int i = 1; i < animator.layerCount; i++) animator.SetLayerWeight(i, gatheringLayerWeights[i]);
        if (stillGathering && (!interrupted || !leaving)) { animator.CrossFade(previousState, .1f, 0); SetBusy(false); }
        else if (!interrupted) SetBusy(false);
        gatheringHash = 0; gatheringLayerWeights = null;
        RefreshEquipmentPoses();
    }
    public void Death() => PlayAction("Death", true);
}
