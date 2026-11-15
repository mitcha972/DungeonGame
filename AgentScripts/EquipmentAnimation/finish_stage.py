from pathlib import Path
import json
base=Path('Temp/EquipmentAnimation/Staged')
manifest=json.loads(Path('Temp/EquipmentAnimation/sources.json').read_text())
def update(path,old,new):
 p=base/path
 if not p.exists():
  p.parent.mkdir(parents=True,exist_ok=True)
  p.write_text(Path(path).read_text(encoding='utf-8-sig'))
  manifest.append({'old':path,'new':path})
 s=p.read_text(encoding='utf-8-sig')
 if new and new in s:return
 if old not in s:
  if not new:return
  raise RuntimeError(path+': missing '+old[:70])
 p.write_text(s.replace(old,new),encoding='utf-8')

equipment='Assets/Scripts/Inventory/Items/Equipment.cs'
update(equipment,'CharacterEquipment.MeshBlendShape[]','CharacterEquipment.BodyRegion[]')
update(equipment,'public SkinnedMeshRenderer mesh;','public SkinnedMeshRenderer mesh;\n    [Tooltip("Explicit attacks for this main-hand weapon. Armor and shields leave this empty.")]\n    public WeaponAttackSet weaponAttacks;')
update('Assets/Scripts/Inventory/UI/EquipmentSlotUI.cs','if (!inventory.Add(equipped)) return;\n        equipmentManager.Unequip(equipmentSlot);','if (!equipmentManager.TryUnequipToInventory(equipmentSlot)) return;')
update('Assets/Scripts/Combat/Projectile.cs','    public WeaponAttackSet weaponClass;\n','')
update('Assets/Scripts/Combat/Animation/CharacterAnimation.cs','    private CharacterCombat combat;\n','')
update('Assets/Scripts/Combat/Animation/CharacterAnimation.cs','        combat = GetComponent<CharacterCombat>();\n','')

p='Assets/Scripts/Combat/CharacterCombat.cs'
update(p,'public GameObject currentProjectile, bloodSplat;','public GameObject bloodSplat;')
update(p,'    protected WeaponAttackSet ActiveWeapon => _combatAnimator != null ? _combatAnimator.CurrentWeapon : null;', '''    private CharacterEquipment equipment;
    protected CharacterEquipment Equipment => equipment;
    public WeaponAttackSet CurrentAttackSet { get; private set; }
    public AttackMotion CurrentAttackMotion { get; private set; }
    public float AttackDuration { get; private set; }
    protected GameObject AttackProjectile { get; private set; }
    protected float AttackProjectileSpeed { get; private set; }
    protected float AttackProjectileRange { get; private set; }
    private float damageStart, damageEnd, attackInterval;''')
update(p,'        _combatAnimator = GetComponent<CharacterAnimation>();','        _combatAnimator = GetComponent<CharacterAnimation>();\n        equipment = GetComponentInChildren<CharacterEquipment>(true);')
update(p,'    protected virtual float AttackCooldown => ActiveWeapon != null ? ActiveWeapon.cooldown : 0.8f;', '    protected virtual float GetAttackInterval(WeaponAttackSet attacks) => attacks.attackInterval;')
update(p,'    protected virtual void OnEnable() { if (myStats != null && !myStats.IsDead) attackIsReady = true; }\n    protected virtual void OnDisable() => CancelAttack();', '''    protected virtual void OnEnable()
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
    }''')
update(p,'        IsExecutingAttack = false;\n        _damageWindow = false;\n        InCombat = isBlocking = false;', '''        IsExecutingAttack = false;
        CurrentAttackSet = null;
        CurrentAttackMotion = null;
        AttackProjectile = null;
        _damageWindow = false;
        InCombat = isBlocking = false;''')
update(p,'''        var weapon = ActiveWeapon;
        _ranged = weapon != null && weapon.isRanged;
        _projectileReleased = false;
        _attackDamage = Mathf.Max(1, Mathf.RoundToInt(myStats.EffectiveDamage * (heavy && weapon != null ? weapon.heavyDamageMultiplier : 1f)));''','''        var attacks = equipment != null ? equipment.ActiveAttackSet : null;
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
        _attackDamage = Mathf.Max(1, Mathf.RoundToInt(myStats.EffectiveDamage * (heavy ? attacks.heavyDamageMultiplier : 1f)));''')
update(p,'''        OnAttack?.Invoke(heavy);
        _combatAnimator.SetBusy(true);
        float duration = Mathf.Max(0.1f, _combatAnimator.CurrentAttackDuration);
        _attackRoutine = StartCoroutine(ExecuteAttack(weapon, duration));''','''        _combatAnimator.SetBusy(true);
        _combatAnimator.PlayAttack(motion);
        OnAttack?.Invoke(heavy);
        if (IsExecutingAttack) _attackRoutine = StartCoroutine(ExecuteAttack());''')
update(p,'''    private IEnumerator ExecuteAttack(WeaponAttackSet weapon, float duration)
    {
        float hitStart = weapon != null ? Mathf.Clamp01(weapon.hitStart) : 0.3f;
        float hitEnd = weapon != null ? Mathf.Clamp(Mathf.Max(hitStart, weapon.hitEnd), hitStart, 1f) : 0.6f;''','''    private IEnumerator ExecuteAttack()
    {
        float duration = AttackDuration;
        float hitStart = damageStart, hitEnd = damageEnd;''')
update(p,'AttackCooldown - duration','attackInterval - duration')
update(p,'        _combatAnimator.PlayAbilityAnimation(abilities.startAnimation);','        if (!_combatAnimator.PlayAbilityAnimation(abilities)) return;')
update(p,'        var equipment = GetComponent<CharacterEquipment>();\n','')

p='Assets/Scripts/Combat/PlayerCombat.cs'
update(p,'    private CharacterEquipment _equipment;\n','')
update(p,'_equipment != null && _equipment.Items','Equipment != null && Equipment.Items')
update(p,'        _equipment = GetComponent<CharacterEquipment>();\n','')
update(p,'''        if (!TryReleaseProjectile() || ActiveWeapon == null || ActiveWeapon.projectile == null) return;
        SpawnProjectile(ActiveWeapon.projectile, AttackDamage, ActiveWeapon.projectileRange, ActiveWeapon.projectileSpeed);''','''        if (!TryReleaseProjectile() || AttackProjectile == null) return;
        SpawnProjectile(AttackProjectile, AttackDamage, AttackProjectileRange, AttackProjectileSpeed);''')
p='Assets/Scripts/Combat/EnemyCombat.cs'
update(p,'protected override float AttackCooldown => myStats.attackSpeed > 0 ? 1f / myStats.attackSpeed : base.AttackCooldown;','protected override float GetAttackInterval(WeaponAttackSet attacks) => myStats.attackSpeed > 0 ? 1f / myStats.attackSpeed : base.GetAttackInterval(attacks);')
update(p,'''        if (!HasValidAttackTarget() || ActiveWeapon == null || ActiveWeapon.projectile == null || !TryReleaseProjectile()) return;
        currentProjectile = ActiveWeapon.projectile;
        SpawnProjectile(currentProjectile, AttackDamage, ActiveWeapon.projectileRange, ActiveWeapon.projectileSpeed);''','''        if (!HasValidAttackTarget() || AttackProjectile == null || !TryReleaseProjectile()) return;
        SpawnProjectile(AttackProjectile, AttackDamage, AttackProjectileRange, AttackProjectileSpeed);''')

p='Assets/Scripts/Combat/AttackHitBox.cs'
s=(Path(p)).read_text()
s=s.replace('    private CharacterCombat _combat;','    private CharacterCombat _combat;\n    private Vector3 defaultCenter, defaultSize;')
s=s.replace('        _collider = GetComponent<BoxCollider>();','        InitializeCollider();',1)
s=s.replace('    private void FixedUpdate()', '''    private void InitializeCollider()
    {
        if (_collider != null) return;
        _collider = GetComponent<BoxCollider>();
        defaultCenter = _collider.center;
        defaultSize = _collider.size;
    }
    public SkinnedMeshRenderer WeaponVisual => _weaponMesh;
    private void FixedUpdate()''')
s=s.replace('''        _weaponMesh = mesh;
        if (_bakedMesh == null) _bakedMesh = new Mesh();'''.replace('\n+','\n'),'''        InitializeCollider();
        DisableDamage();
        // Keep the debug volume alive when the previous weapon visual is destroyed.
        if (_visual != null) _visual.transform.SetParent(transform, false);
        _weaponMesh = mesh;
        _collider.center = defaultCenter;
        _collider.size = defaultSize;
        if (mesh != null && mesh.TryGetComponent<BoxCollider>(out var authoredBounds))
        { _collider.center = authoredBounds.center; _collider.size = authoredBounds.size; }
        if (mesh != null && _bakedMesh == null) _bakedMesh = new Mesh();''')
dest=base/p;dest.parent.mkdir(parents=True,exist_ok=True);dest.write_text(s)
p='Assets/Scripts/Combat/Data/Abilities.cs'
s=Path(p).read_text().replace('    public AnimationClip castAnimation;','    public AnimationClip castAnimation;\n    [SerializeField, HideInInspector] private string animationState;\n    public string AnimationState => animationState;')
dest=base/p;dest.parent.mkdir(parents=True,exist_ok=True);dest.write_text(s)

for path in ['Assets/Scripts/Combat/AttackHitBox.cs','Assets/Scripts/Combat/Data/Abilities.cs']:
 if not any(m['old']==path for m in manifest):manifest.append({'old':path,'new':path})
Path('Temp/EquipmentAnimation/sources.json').write_text(json.dumps(manifest,indent=2))
print('Staged equipment transactions, direct attack playback, immutable attack execution values and hitbox reset.')
