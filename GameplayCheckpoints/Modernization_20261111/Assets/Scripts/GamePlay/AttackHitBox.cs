using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AttackHitBox : MonoBehaviour
{
    public CharacterStats myStats;
    public CharacterCombat target;
    public GameObject owner;
    public List<CharacterStats> collidedCharacters = new List<CharacterStats>();
    private BoxCollider _collider;
    private CharacterCombat _combat;
    private readonly Collider[] _overlaps = new Collider[64];
    private readonly HashSet<DestructibleObject> _destructibles = new HashSet<DestructibleObject>();
    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = true;
        _collider.enabled = false;
        _combat = GetComponentInParent<CharacterCombat>();
        if (myStats == null && _combat != null) myStats = _combat.myStats;
        if (owner == null && _combat != null) owner = _combat.gameObject;
    }
    private void FixedUpdate()
    {
        if (!_collider.enabled || myStats == null || myStats.IsDead) return;
        Transform hitTransform = transform;
        Bounds bounds = new Bounds(_collider.center, _collider.size);
        if (_weaponMesh != null)
        {
            _weaponMesh.BakeMesh(_bakedMesh);
            _bakedMesh.RecalculateBounds();
            bounds = _bakedMesh.bounds;
            hitTransform = _weaponMesh.transform;
        }
        Vector3 scale = hitTransform.lossyScale;
        Vector3 halfExtents = Vector3.Scale(bounds.extents, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        int count = Physics.OverlapBoxNonAlloc(hitTransform.TransformPoint(bounds.center), halfExtents, _overlaps, hitTransform.rotation, ~0, QueryTriggerInteraction.Ignore);
        Collider[] overlaps = _overlaps;
        if (count == _overlaps.Length)
        {
            overlaps = Physics.OverlapBox(hitTransform.TransformPoint(bounds.center), halfExtents, hitTransform.rotation, ~0, QueryTriggerInteraction.Ignore);
            count = overlaps.Length;
        }
        for (int i = 0; i < count; i++) Hit(overlaps[i]);
    }
    private SkinnedMeshRenderer _weaponMesh;
    private Mesh _bakedMesh;
    public void ConfigureWeapon(SkinnedMeshRenderer mesh)
    {
        _weaponMesh = mesh;
        if (_bakedMesh == null) _bakedMesh = new Mesh();
    }
    private void OnDestroy() { if (_bakedMesh != null) Destroy(_bakedMesh); }
    private void OnTriggerEnter(Collider other) { if (_collider.enabled && _weaponMesh == null) Hit(other); }
    private void Hit(Collider other)
    {
        if (owner == null || other.transform.IsChildOf(owner.transform)) return;
        var stats = other.GetComponentInParent<CharacterStats>();
        if (stats != null)
        {
            if (stats == myStats || stats.IsDead || stats.CompareTag(owner.tag) || collidedCharacters.Contains(stats)) return;
            target = stats.GetComponent<CharacterCombat>();
            if (target == null) return;
            collidedCharacters.Add(stats);
            Vector3 direction = owner.transform.position - stats.transform.position;
            direction.y = 0f;
            float angle = Vector3.SignedAngle(direction, stats.transform.forward, Vector3.up);
            bool blocked = target.isBlocking && Vector3.Dot(stats.transform.forward, direction.normalized) > 0.2f;
            target.TakeDamage(_combat != null ? _combat.AttackDamage : Mathf.Max(1, myStats.damage), owner.transform, angle, other.ClosestPoint(transform.position), blocked);
            return;
        }
        var destructible = other.GetComponentInParent<DestructibleObject>();
        if (destructible != null && _destructibles.Add(destructible)) destructible.Damage(Mathf.Max(1, myStats.damage), transform.position);
    }
    public void BeginAttack()
    {
        collidedCharacters.Clear();
        _destructibles.Clear();
        target = null;
    }
    public void EnableDamage() { if (_collider != null && myStats != null && !myStats.IsDead && _combat != null && _combat.isActiveAndEnabled) _collider.enabled = true; }
    public void DisableDamage()
    {
        if (_collider != null) _collider.enabled = false;
        target = null;
    }
    public void UpdateAttackHitBox(Equipment equipment) { }
}
