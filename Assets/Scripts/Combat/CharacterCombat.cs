using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    public event System.Action<bool> OnAttack;
    public CharacterStats myStats;
    public GameObject bloodSplat;
    public AttackHitBox attackHitBox;
    public Abilities abilities;
    public bool InCombat, canAttack, attackIsReady;
    public BlockHitBox blockHitBox;
    public bool isBlocking, canBlock;
    public float cooldownTime;
    [SerializeField] private float hitRecoveryTime = 0.3f;
    private CharacterAnimation _combatAnimator;
    private Coroutine _attackRoutine;
    private bool _damageWindow, _projectileReleased, _ranged;
    private int _attackDamage;
    public int AttackDamage => _attackDamage;
    public bool IsExecutingAttack { get; private set; }
    private CharacterEquipment equipment;
    protected CharacterEquipment Equipment => equipment;
    public WeaponAttackSet CurrentAttackSet { get; private set; }
    public AttackMotion CurrentAttackMotion { get; private set; }
    public float AttackDuration { get; private set; }
    protected GameObject AttackProjectile { get; private set; }
    protected float AttackProjectileSpeed { get; private set; }
    protected float AttackProjectileRange { get; private set; }
    private float damageStart, damageEnd, attackInterval;

    protected virtual void Awake()
    {
        myStats = GetComponent<CharacterStats>();
        _combatAnimator = GetComponent<CharacterAnimation>();
        equipment = GetComponentInChildren<CharacterEquipment>(true);
        if (attackHitBox == null) attackHitBox = GetComponentInChildren<AttackHitBox>(true);
        if (blockHitBox == null) blockHitBox = GetComponentInChildren<BlockHitBox>(true);
        if (attackHitBox != null) { attackHitBox.owner = gameObject; attackHitBox.myStats = myStats; attackHitBox.DisableDamage(); }
    }
    protected virtual float GetAttackInterval(WeaponAttackSet attacks) => attacks.attackInterval;
    private void Start() { canAttack = canBlock = attackIsReady = !myStats.IsDead; }
    protected virtual void OnEnable()
    {
        if (myStats != null && !myStats.IsDead) attackIsReady = true;
        if (equipment != null) equipment.EquipmentChanged += OnEquipmentChanged;
    }
    protected virtual void OnDisable()
    {
        if (equipment != null) equipment.EquipmentChanged -= OnEquipmentChanged;
        CancelAttack();
    }
    private void OnEquipmentChanged(Equipment item, Equipment previous)
    {
        if ((item != null && item.equipSlot == EquipmentSlot.MainHand) || (previous != null && previous.equipSlot == EquipmentSlot.MainHand)) CancelAttack();
        if ((item != null && item.equipSlot == EquipmentSlot.OffHand) || (previous != null && previous.equipSlot == EquipmentSlot.OffHand)) BlockDrop();
    }
    public void CancelAttack()
    {
        StopAllCoroutines();
        _attackRoutine = null;
        IsExecutingAttack = false;
        _combatAnimator?.CancelAttackMotion(CurrentAttackMotion);
        CurrentAttackSet = null;
        CurrentAttackMotion = null;
        AttackProjectile = null;
        _damageWindow = false;
        InCombat = isBlocking = false;
        DisableDamage();
        if (blockHitBox != null) blockHitBox.DisableBlock();
        attackIsReady = isActiveAndEnabled && myStats != null && !myStats.IsDead;
        if (_combatAnimator != null && myStats != null && !myStats.IsDead) _combatAnimator.SetBusy(false);
    }
    public virtual void Attack(bool heavy)
    {
        if (!isActiveAndEnabled || !canAttack || !attackIsReady || myStats.IsDead || myStats.currentHealth <= 0 || _combatAnimator == null || _combatAnimator.IsBusy) return;
        if (equipment != null && equipment.IsChanging) return;
        var attacks = equipment != null ? equipment.ActiveAttackSet : null;
        var motion = attacks != null ? attacks.SelectMotion(heavy) : null;
        if (!_combatAnimator.CanPlayAttack(motion)) return;
        // Capture all execution values now; equipment/animation changes cannot retarget an in-flight hit.
        CurrentAttackSet = attacks;
        CurrentAttackMotion = motion;
        AttackDuration = Mathf.Max(.1f, motion.clip.length);
        damageStart = Mathf.Clamp01(attacks.damageWindowStart);
        damageEnd = Mathf.Clamp(attacks.damageWindowEnd, damageStart, 1f);
        attackInterval = Mathf.Max(0f, GetAttackInterval(attacks));
        _ranged = attacks.launchesProjectile;
        AttackProjectile = attacks.projectilePrefab;
        AttackProjectileSpeed = attacks.projectileSpeed;
        AttackProjectileRange = attacks.projectileRange;
        if (_ranged && AttackProjectile == null) return;
        _projectileReleased = false;
        _attackDamage = Mathf.Max(1, Mathf.RoundToInt(myStats.EffectiveDamage * (heavy ? attacks.heavyDamageMultiplier : 1f)));
        attackIsReady = false;
        IsExecutingAttack = true;
        InCombat = true;
        if (attackHitBox != null) attackHitBox.BeginAttack();
        _combatAnimator.SetBusy(true);
        _combatAnimator.PlayAttack(motion);
        OnAttack?.Invoke(heavy);
        if (IsExecutingAttack) StartCoroutine(ExecuteAttack());
    }
    private IEnumerator ExecuteAttack()
    {
        float duration = AttackDuration;
        float hitStart = damageStart, hitEnd = damageEnd;
        float elapsed = 0f;
        float previousPhase = -1f;
        while (true)
        {
            if (myStats.IsDead) break;
            float phase = elapsed / duration;
            _damageWindow = CrossesDamageWindow(previousPhase, phase, hitStart, hitEnd);
            if (_ranged)
            {
                if (_damageWindow && !_projectileReleased) EnableProjectile();
            }
            else if (_damageWindow)
            {
                EnableDamage();
                if (attackHitBox != null) attackHitBox.SampleDamage();
            }
            else DisableDamage();
            if (elapsed >= duration) break;
            previousPhase = phase;
            yield return null;
            elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
        }
        IsExecutingAttack = false;
        _damageWindow = false;
        DisableDamage();
        if (!myStats.IsDead) _combatAnimator.SetBusy(false);
        cooldownTime = Mathf.Max(0f, attackInterval - duration);
        while (cooldownTime > 0f) { cooldownTime -= Time.deltaTime; yield return null; }
        attackIsReady = !myStats.IsDead;
        _attackRoutine = null;
    }
    // Inclusive interval intersection also catches a window skipped by a slow frame.
    public static bool CrossesDamageWindow(float previousPhase, float phase, float hitStart, float hitEnd)
    {
        return phase >= hitStart && previousPhase <= hitEnd && phase >= previousPhase;
    }
    protected bool TryReleaseProjectile()
    {
        if (!isActiveAndEnabled || !_ranged || !_damageWindow || _projectileReleased || myStats.IsDead) return false;
        _projectileReleased = true;
        return true;
    }
    public void Ability()
    {
        if (!isActiveAndEnabled || abilities == null || _combatAnimator == null || _combatAnimator.IsBusy || myStats.IsDead) return;
        if (!_combatAnimator.PlayAbilityAnimation(abilities)) return;
        if (abilities.projectile != null) EnableProjectile(abilities, abilities.damage);
        StartCoroutine(RecoverHit(abilities.startAnimation != null ? Mathf.Max(hitRecoveryTime, abilities.startAnimation.length) : hitRecoveryTime));
    }
    public virtual void TakeDamage(int damageTaken, Transform attackOwner, float direction, Vector3 position, bool blocked)
    {
        if (!isActiveAndEnabled || myStats.IsDead || damageTaken <= 0) return;
        int previousHealth = myStats.currentHealth;
        Equipment shield = null;
        bool hasShield = equipment != null && equipment.Items.TryGetValue(EquipmentSlot.OffHand, out shield)
            && shield != null && shield.itemType == ItemType.Shield;
        blocked = blocked && isBlocking && hasShield;
        // Shield armor is passive equipment armor; an active frontal block adds its value again.
        int incoming = blocked ? Mathf.Max(1, damageTaken - Mathf.Max(0, shield.armorModifier)) : damageTaken;
        myStats.TakeDamage(incoming, attackOwner, direction);
        if (blocked && isBlocking && damageTaken > incoming && myStats.currentHealth < previousHealth && !myStats.IsDead && attackOwner != null)
        {
            var attacker = attackOwner.GetComponentInParent<EnemyStats>();
            var progress = GetComponent<PlayerSkillProgress>();
            if (attacker != null && attacker.isActiveAndEnabled && !attacker.IsDead && !attacker.CompareTag(gameObject.tag) && progress != null)
                progress.AwardXP(RpgSkill.Blocking, progress.successfulBlockXp, position);
        }
        if (myStats.IsDead) return;
        StopAllCoroutines();
        _attackRoutine = null;
        IsExecutingAttack = false;
        _damageWindow = false;
        DisableDamage();
        attackIsReady = false;
        if (_combatAnimator != null)
        {
            _combatAnimator.SetBusy(true);
            if (blocked) _combatAnimator.PlayAction("BlockImpact", true); else _combatAnimator.GetHit(direction);
        }
        StartCoroutine(RecoverHit());
        if (!blocked && bloodSplat != null) Destroy(Instantiate(bloodSplat, position, Quaternion.identity), 2f);
    }
    private IEnumerator RecoverHit(float duration = -1f)
    {
        yield return new WaitForSeconds(duration >= 0f ? duration : hitRecoveryTime);
        if (!myStats.IsDead) { attackIsReady = true; if (_combatAnimator != null) _combatAnimator.SetBusy(false); }
    }
    public void BlockStart()
    {
        if (!isActiveAndEnabled || !canBlock || isBlocking || _combatAnimator == null || _combatAnimator.IsBusy || myStats.IsDead) return;
        isBlocking = true;
        if (blockHitBox != null) blockHitBox.EnableBlock();
        _combatAnimator.PlayAction("BlockStart", false);
    }
    public void BlockDrop()
    {
        if (!isBlocking) return;
        isBlocking = false;
        if (blockHitBox != null) blockHitBox.DisableBlock();
        if (_combatAnimator != null) _combatAnimator.PlayAction("BlockDrop", false);
    }
    public virtual void EnableProjectile() { }
    public virtual void EnableProjectile(Abilities ability, int damage) { }
    public void EnableDamage() { if (isActiveAndEnabled && !myStats.IsDead && _damageWindow && !_ranged && attackHitBox != null) attackHitBox.EnableDamage(); }
    public void DisableDamage() { if (attackHitBox != null) attackHitBox.DisableDamage(); }
}
