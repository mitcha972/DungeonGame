using UnityEngine;

public class EnemyCombat : CharacterCombat
{
    private EnemyAi _ai;
    protected override void Awake()
    {
        base.Awake();
        _ai = GetComponent<EnemyAi>();
    }
    // Existing attackSpeed is interpreted as attacks per second; zero uses the weapon cooldown.
    protected override float AttackCooldown => myStats.attackSpeed > 0 ? 1f / myStats.attackSpeed : base.AttackCooldown;
    private bool HasValidAttackTarget()
    {
        if (!isActiveAndEnabled || _ai == null || !_ai.isActiveAndEnabled || _ai.target == null || myStats.IsDead) return false;
        var targetStats = _ai.target.GetComponentInParent<CharacterStats>();
        return targetStats != null && targetStats.isActiveAndEnabled && !targetStats.IsDead && targetStats.currentHealth > 0
            && Vector3.Distance(transform.position, _ai.target.position) <= Mathf.Max(0.1f, myStats.range)
            && _ai.CanSeeTarget() && _ai.IsFacingTarget();
    }
    public override void Attack(bool heavy)
    {
        if (HasValidAttackTarget()) base.Attack(heavy);
    }
    public override void EnableProjectile()
    {
        if (!HasValidAttackTarget() || ActiveWeapon == null || ActiveWeapon.projectile == null || !TryReleaseProjectile()) return;
        currentProjectile = ActiveWeapon.projectile;
        SpawnProjectile(currentProjectile, AttackDamage, ActiveWeapon.projectileRange, ActiveWeapon.projectileSpeed);
    }
    public override void EnableProjectile(Abilities ability, int damage)
    {
        if (!HasValidAttackTarget() || ability == null || ability.projectile == null) return;
        SpawnProjectile(ability.projectile, damage, ability.range, ability.speed);
    }
    private void SpawnProjectile(GameObject prefab, int damage, float range, float speed)
    {
        var projectile = Instantiate(prefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        if (!projectile.TryGetComponent<Projectile>(out var hitbox) || !projectile.TryGetComponent<Rigidbody>(out var body))
        {
            Debug.LogError("Enemy projectile requires Projectile and Rigidbody components.", prefab);
            Destroy(projectile);
            return;
        }
        projectile.tag = gameObject.tag;
        projectile.transform.LookAt(_ai.target.position + Vector3.up);
        hitbox.StartUp(myStats, gameObject, Mathf.Max(1, damage), range, speed);
    }
}
