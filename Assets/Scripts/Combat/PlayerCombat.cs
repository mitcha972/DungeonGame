using UnityEngine;

public class PlayerCombat : CharacterCombat
{
    [SerializeField] private Transform aim;
    private Camera _camera;
    private EquipmentManager _equipment;
    private PlayerControlState _state;
    private Animator _animator;
    public bool HasShield => _equipment != null && _equipment.currentEquipment.TryGetValue(EquipmentSlot.OffHand, out Equipment item) && item != null && item.itemType == ItemType.Shield && blockHitBox != null;
    public bool SupportsPowerAttack => false;
    public override void Attack(bool heavy) => base.Attack(false);
    protected override void Awake()
    {
        base.Awake();
        _camera = Camera.main;
        _equipment = GetComponent<EquipmentManager>();
        _state = GetComponent<PlayerControlState>();
        _animator = GetComponentInChildren<Animator>();
    }
    protected override void OnDisable() { BlockDrop(); base.OnDisable(); }
    private void Update()
    {
        if (isBlocking && (!HasShield || myStats.IsDead || (_state != null && !_state.GameplayAllowed))) BlockDrop();
        if (_animator != null) _animator.SetBool("Blocking", isBlocking);
    }
    public void SetShieldBlock(bool held)
    {
        if (held && HasShield && !myStats.IsDead && (_state == null || _state.GameplayAllowed)) BlockStart();
        else BlockDrop();
    }
    public override void EnableProjectile()
    {
        if (!TryReleaseProjectile() || ActiveWeapon == null || ActiveWeapon.projectile == null) return;
        SpawnProjectile(ActiveWeapon.projectile, AttackDamage, ActiveWeapon.projectileRange, ActiveWeapon.projectileSpeed);
    }
    public override void EnableProjectile(Abilities ability, int damage)
    {
        if (ability != null && ability.projectile != null) SpawnProjectile(ability.projectile, damage, ability.range, ability.speed);
    }
    private void SpawnProjectile(GameObject prefab, int damage, float range, float speed)
    {
        Vector3 spawn = attackHitBox != null ? attackHitBox.transform.position : transform.position + Vector3.up;
        Vector3 destination = spawn + transform.forward * range;
        if (_camera != null)
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            int aimMask = ~(1 << gameObject.layer);
            destination = Physics.Raycast(ray, out RaycastHit hit, range, aimMask, QueryTriggerInteraction.Ignore) ? hit.point : ray.GetPoint(range);
        }
        Vector3 direction = (destination - spawn).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = transform.forward;
        var instance = Instantiate(prefab, spawn, Quaternion.LookRotation(direction));
        if (!instance.TryGetComponent<Projectile>(out var projectile))
        {
            Debug.LogError("Ranged prefab needs Projectile.", prefab);
            Destroy(instance);
            return;
        }
        projectile.StartUp(myStats, gameObject, damage, range, speed);
    }
}

