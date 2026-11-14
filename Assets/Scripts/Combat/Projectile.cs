using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public CharacterStats myStats;
    [HideInInspector] public CharacterCombat target;
    [HideInInspector] public GameObject owner;
    public WeaponClass weaponClass;
    public GameObject collisionEffect;
    [HideInInspector] public float speed;
    public bool rangeEnd = true;
    public bool destroyOnCollide = true;
    public LayerMask solidCollisionMask;
    [SerializeField] private float collisionRadius = 0.05f;
    [SerializeField] private float lifetime = 10f;
    private Material _attackVisualMaterial;
    private int _damage;
    private float _range, _travelled;
    private Vector3 _previousPosition;
    private Rigidbody _body;
    private bool _initialized, _consumed;
    private readonly HashSet<CharacterStats> _hitCharacters = new HashSet<CharacterStats>();
    private readonly RaycastHit[] _hits = new RaycastHit[32];
    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _previousPosition = transform.position;
        Destroy(gameObject, lifetime);
    }
    public void StartUp(CharacterStats stats, GameObject source, int damage, float range, float velocity)
    {
        myStats = stats;
        owner = source;
        _damage = Mathf.Max(1, damage);
        _range = Mathf.Max(1f, range);
        speed = Mathf.Max(1f, velocity);
        _initialized = true;
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Projectile Attack Box";
        Destroy(visual.GetComponent<Collider>());
        visual.transform.SetParent(transform, false);
        // Sweep uses a world-space radius, independent of the prefab scale.
        Vector3 scale = transform.lossyScale;
        float size = Mathf.Max(.1f, collisionRadius * 2f);
        visual.transform.localScale = new Vector3(size / Mathf.Max(.001f, Mathf.Abs(scale.x)), size / Mathf.Max(.001f, Mathf.Abs(scale.y)), size / Mathf.Max(.001f, Mathf.Abs(scale.z)));
        _attackVisualMaterial = new Material(Shader.Find("Sprites/Default"));
        _attackVisualMaterial.color = new Color(1f, .65f, .05f, .5f);
        var renderer = visual.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = _attackVisualMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        _body.isKinematic = false;
        _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _body.linearVelocity = transform.forward * speed;
        if (owner != null)
        {
            var projectileColliders = GetComponentsInChildren<Collider>();
            foreach (var ownCollider in owner.GetComponentsInChildren<Collider>())
                foreach (var projectileCollider in projectileColliders)
                    Physics.IgnoreCollision(ownCollider, projectileCollider);
        }
    }
    private void FixedUpdate()
    {
        if (!_initialized || _consumed) return;
        Vector3 position = _body.position;
        Vector3 delta = position - _previousPosition;
        float distance = delta.magnitude;
        if (distance > 0.001f)
        {
            int count = Physics.SphereCastNonAlloc(_previousPosition, collisionRadius, delta / distance, _hits, distance, ~0, QueryTriggerInteraction.Ignore);
            RaycastHit[] hits = _hits;
            if (count == _hits.Length)
            {
                // A full nonalloc buffer may omit the nearest wall or target.
                hits = Physics.SphereCastAll(_previousPosition, collisionRadius, delta / distance, distance, ~0, QueryTriggerInteraction.Ignore);
                count = hits.Length;
            }
            // Resolve nearest blocking hit first, independent of physics result order.
            for (int pass = 0; pass < count && !_consumed; pass++)
            {
                int nearest = -1;
                float best = float.PositiveInfinity;
                for (int i = 0; i < count; i++) if (hits[i].collider != null && hits[i].distance < best) { nearest = i; best = hits[i].distance; }
                if (nearest < 0) break;
                var hit = hits[nearest];
                hits[nearest] = default;
                Hit(hit.collider, hit.point);
            }
            _travelled += distance;
        }
        _previousPosition = position;
        if (!_consumed && rangeEnd && _travelled >= _range) Finish(position);
    }
    private void OnTriggerEnter(Collider other) { if (_initialized && !_consumed) Hit(other, transform.position); }
    private void OnCollisionEnter(Collision collision) { if (_initialized && !_consumed) Hit(collision.collider, collision.contactCount > 0 ? collision.GetContact(0).point : transform.position); }
    private void Hit(Collider other, Vector3 position)
    {
        if (owner == null || other.transform.IsChildOf(owner.transform) || other.transform.IsChildOf(transform)) return;
        var stats = other.GetComponentInParent<CharacterStats>();
        if (stats != null)
        {
            if (stats == myStats || stats.IsDead || stats.CompareTag(owner.tag) || !_hitCharacters.Add(stats)) return;
            target = stats.GetComponent<CharacterCombat>();
            if (target != null)
            {
                Vector3 direction = owner.transform.position - stats.transform.position;
                float angle = Vector3.SignedAngle(direction, stats.transform.forward, Vector3.up);
                bool blocked = target.isBlocking && Vector3.Dot(stats.transform.forward, direction.normalized) > 0.2f;
                int previousHealth = stats.currentHealth;
                target.TakeDamage(_damage, owner.transform, angle, position, blocked);
                owner.GetComponent<PlayerSkillProgress>()?.AwardDamage(stats, previousHealth - stats.currentHealth, true, position);
            }
            if (destroyOnCollide) Finish(position);
            return;
        }
        var destructible = other.GetComponentInParent<DestructibleObject>();
        if (destructible != null) destructible.Damage(_damage, position);
        if (!other.isTrigger || destructible != null) Finish(position);
    }
    private void OnDestroy() { if (_attackVisualMaterial != null) Destroy(_attackVisualMaterial); }
    private void Finish(Vector3 position)
    {
        if (_consumed) return;
        _consumed = true;
        _body.linearVelocity = Vector3.zero;
        if (collisionEffect != null) Destroy(Instantiate(collisionEffect, position, Quaternion.identity), 2f);
        Destroy(gameObject);
    }
}
