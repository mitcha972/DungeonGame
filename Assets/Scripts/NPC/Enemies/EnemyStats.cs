using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : CharacterStats
{
    [SerializeField] private Enemy enemy;
    private EnemyAi _enemyAi;
    private bool _deathHandled;
    public Enemy Definition => enemy;
    public event System.Action<Transform> OnDeath;

    private void Awake()
    {
        equipmentManager = GetComponentInChildren<EquipmentManager>(true);
        if (equipmentManager != null) equipmentManager.onEquipmentChanged += OnEquipmentChanged;
        _enemyAi = GetComponent<EnemyAi>();
        if (enemy == null)
        {
            Debug.LogError("Assign an Enemy definition to EnemyStats.", this);
            if (_enemyAi != null) _enemyAi.enabled = false;
            enabled = false;
            return;
        }
        maxHealth = Mathf.Max(1, enemy.health);
        damage = Mathf.Max(0, enemy.damage);
        armor = Mathf.Max(0, enemy.armor);
        range = Mathf.Max(1, enemy.attackRange);
        attackSpeed = Mathf.Max(0, enemy.attackSpeed);
        movementSpeed = Mathf.Max(0, enemy.movementSpeed);
        sightRange = Mathf.Max(0, enemy.sightRange);
        StartUP();
    }

    private void OnDestroy()
    {
        if (equipmentManager != null) equipmentManager.onEquipmentChanged -= OnEquipmentChanged;
    }

    public override void TakeDamage(int damageTaken, Transform attackOwner, float attackDirection)
    {
        if (!isActiveAndEnabled || IsDead || currentHealth <= 0 || damageTaken <= 0) return;
        base.TakeDamage(damageTaken, attackOwner, attackDirection);
        if (!IsDead && _enemyAi != null) _enemyAi.NotifyDamaged(attackOwner);
    }

    public override void Die(Transform attackOwner)
    {
        if (_deathHandled) return;
        _deathHandled = true;
        currentHealth = 0;
        base.Die(attackOwner);
        if (_enemyAi != null) _enemyAi.Die();
        else
        {
            var combat = GetComponent<CharacterCombat>();
            if (combat != null) combat.enabled = false;
        }
        if (enemy != null)
        {
            var playerStats = attackOwner != null ? attackOwner.GetComponentInParent<PlayerStats>() : null;
            if (playerStats != null) playerStats.AddXP(Mathf.Max(0, enemy.XP));
            DropLoot();
        }
        OnDeath?.Invoke(attackOwner);
    }

    private void DropLoot()
    {
        if (enemy.itemDrops == null) return;
        Vector3 position = transform.position;
        var agent = GetComponent<NavMeshAgent>();
        var filter = new NavMeshQueryFilter { agentTypeID = agent != null ? agent.agentTypeID : 0, areaMask = agent != null ? agent.areaMask : NavMesh.AllAreas };
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, filter)) position = hit.position;
        foreach (var drop in enemy.itemDrops)
        {
            if (drop == null || drop.lootItem == null || Random.Range(0, 100) >= Mathf.Clamp(drop.dropChance, 0, 100)) continue;
            var item = Instantiate(drop.lootItem, position + Vector3.up * 0.5f, Quaternion.identity);
            if (item.TryGetComponent<Rigidbody>(out var body) && !body.isKinematic)
                body.AddForce(Vector3.up * Random.Range(1f, 2f), ForceMode.Impulse);
        }
    }
}
