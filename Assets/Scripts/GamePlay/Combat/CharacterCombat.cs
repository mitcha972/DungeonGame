using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    public event System.Action<bool> OnAttack;
    public CharacterStats myStats;
    public GameObject currentProjectile, bloodSplat;
    public AttackHitBox attackHitBox;
    public Abilities abilities;
    public bool InCombat, canAttack, attackIsReady;
    public BlockHitBox blockHitBox;
    public bool isBlocking, canBlock;
    public float cooldownTime;
    [SerializeField, Range(0f, 1f)] private float blockDamageMultiplier = 0.35f;
    [SerializeField] private float hitRecoveryTime = 0.3f;
    private CharacterAnimator _combatAnimator;
    private Coroutine _attackRoutine;
    private bool _damageWindow, _projectileReleased, _ranged;
    private int _attackDamage;
    public int AttackDamage => _attackDamage;
    public bool IsExecutingAttack { get; private set; }
    protected WeaponClass ActiveWeapon => _combatAnimator != null ? _combatAnimator.CurrentWeapon : null;

    protected virtual void Awake()
    {
        myStats = GetComponent<CharacterStats>();
        _combatAnimator = GetComponent<CharacterAnimator>();
        if (attackHitBox == null) attackHitBox = GetComponentInChildren<AttackHitBox>(true);
        if (blockHitBox == null) blockHitBox = GetComponentInChildren<BlockHitBox>(true);
        if (attackHitBox != null) { attackHitBox.owner = gameObject; attackHitBox.myStats = myStats; attackHitBox.DisableDamage(); }
    }
    protected virtual float AttackCooldown => ActiveWeapon != null ? ActiveWeapon.cooldown : 0.8f;
    private void Start() { canAttack = canBlock = attackIsReady = !myStats.IsDead; }
    protected virtual void OnEnable() { if (myStats != null && !myStats.IsDead) attackIsReady = true; }
    protected virtual void OnDisable() => CancelAttack();
    public void CancelAttack()
    {
        StopAllCoroutines();
        _attackRoutine = null;
        IsExecutingAttack = false;
        _damageWindow = false;
        InCombat = isBlocking = false;
        DisableDamage();
        if (blockHitBox != null) blockHitBox.DisableBlock();
        attackIsReady = isActiveAndEnabled && myStats != null && !myStats.IsDead;
        if (_combatAnimator != null && myStats != null && !myStats.IsDead) _combatAnimator.SetBusy(false);
    }
    public virtual void Attack(bool heavy)
    {
        if (!isActiveAndEnabled || !canAttack || !attackIsReady || myStats.IsDead || myStats.currentHealth <= 0 || _combatAnimator == null || _combatAnimator.AnimationIsBusy()) return;
        var weapon = ActiveWeapon;
        _ranged = weapon != null && weapon.isRanged;
        _projectileReleased = false;
        _attackDamage = Mathf.Max(1, Mathf.RoundToInt(myStats.damage * (heavy && weapon != null ? weapon.heavyDamageMultiplier : 1f)));
        attackIsReady = false;
        IsExecutingAttack = true;
        InCombat = true;
        if (attackHitBox != null) attackHitBox.BeginAttack();
        OnAttack?.Invoke(heavy);
        _combatAnimator.SetBusy(true);
        float duration = Mathf.Max(0.1f, _combatAnimator.CurrentAttackDuration);
        _attackRoutine = StartCoroutine(ExecuteAttack(weapon, duration));
    }
    private IEnumerator ExecuteAttack(WeaponClass weapon, float duration)
    {
        float hitStart = weapon != null ? Mathf.Clamp01(weapon.hitStart) : 0.3f;
        float hitEnd = weapon != null ? Mathf.Clamp(Mathf.Max(hitStart, weapon.hitEnd), hitStart, 1f) : 0.6f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (myStats.IsDead) break;
            float phase = elapsed / duration;
            _damageWindow = phase >= hitStart && phase <= hitEnd;
            if (_ranged)
            {
                if (_damageWindow && !_projectileReleased) EnableProjectile();
            }
            else if (_damageWindow) EnableDamage();
            else DisableDamage();
            elapsed += Time.deltaTime;
            yield return null;
        }
        IsExecutingAttack = false;
        _damageWindow = false;
        DisableDamage();
        if (!myStats.IsDead) _combatAnimator.SetBusy(false);
        cooldownTime = Mathf.Max(0f, AttackCooldown - duration);
        while (cooldownTime > 0f) { cooldownTime -= Time.deltaTime; yield return null; }
        attackIsReady = !myStats.IsDead;
        _attackRoutine = null;
    }
    protected bool TryReleaseProjectile()
    {
        if (!isActiveAndEnabled || !_ranged || !_damageWindow || _projectileReleased || myStats.IsDead) return false;
        _projectileReleased = true;
        return true;
    }
    public void Ability()
    {
        if (!isActiveAndEnabled || abilities == null || _combatAnimator == null || _combatAnimator.AnimationIsBusy() || myStats.IsDead) return;
        _combatAnimator.PlayAbilityAnimation(abilities.startAnimation);
        if (abilities.projectile != null) EnableProjectile(abilities, abilities.damage);
        StartCoroutine(RecoverHit());
    }
    public virtual void TakeDamage(int damageTaken, Transform attackOwner, float direction, Vector3 position, bool blocked)
    {
        if (!isActiveAndEnabled || myStats.IsDead || damageTaken <= 0) return;
        int incoming = blocked ? Mathf.Max(1, Mathf.RoundToInt(damageTaken * blockDamageMultiplier)) : damageTaken;
        myStats.TakeDamage(incoming, attackOwner, direction);
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
            if (blocked) _combatAnimator.PlayTargetAnimation("BlockImpact", true); else _combatAnimator.GetHit(direction);
        }
        StartCoroutine(RecoverHit());
        if (!blocked && bloodSplat != null) Destroy(Instantiate(bloodSplat, position, Quaternion.identity), 2f);
    }
    private IEnumerator RecoverHit()
    {
        yield return new WaitForSeconds(hitRecoveryTime);
        if (!myStats.IsDead) { attackIsReady = true; if (_combatAnimator != null) _combatAnimator.SetBusy(false); }
    }
    public void BlockStart()
    {
        if (!isActiveAndEnabled || !canBlock || isBlocking || _combatAnimator == null || _combatAnimator.AnimationIsBusy() || myStats.IsDead) return;
        isBlocking = true;
        if (blockHitBox != null) blockHitBox.EnableBlock();
        _combatAnimator.PlayTargetAnimation("BlockStart", false);
    }
    public void BlockDrop()
    {
        if (!isBlocking) return;
        isBlocking = false;
        if (blockHitBox != null) blockHitBox.DisableBlock();
        if (_combatAnimator != null) _combatAnimator.PlayTargetAnimation("BlockDrop", false);
    }
    public virtual void EnableProjectile() { }
    public virtual void EnableProjectile(Abilities ability, int damage) { }
    public void EnableDamage() { if (isActiveAndEnabled && !myStats.IsDead && _damageWindow && !_ranged && attackHitBox != null) attackHitBox.EnableDamage(); }
    public void DisableDamage() { if (attackHitBox != null) attackHitBox.DisableDamage(); }
}
