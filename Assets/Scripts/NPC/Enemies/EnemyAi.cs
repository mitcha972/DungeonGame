using UnityEngine;
using UnityEngine.AI;

/// <summary>Shared enemy state and navigation owner. Stats and tuning come from EnemyStats.Definition.</summary>
[RequireComponent(typeof(NavMeshAgent), typeof(EnemyStats))]
public class EnemyAi : MonoBehaviour
{
    public LayerMask whatIsGround, whatIsPlayer, ignoreLayers;
    public Transform target;
    public float distanceToLastCorner, currentSpeed;
    public bool walkPointSet;
    [SerializeField] private float repathInterval = 0.4f;
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float memoryDuration = 5f;
    [SerializeField] private float turnSpeed = 360f;
    // Preserve existing serialized state values.
    public enum State { Patrol, Idle, InCombat, Attacking, ResetAttackPosition, SearchingForTarget, Dead, PreparingAttack, TakingDamage, Dying }
    [SerializeField] private State CurrentState = State.Idle;
    public State ActiveState => CurrentState;
    [Header("Optional patrol")]
    [SerializeField] private bool patrolEnabled;
    [SerializeField] private Transform[] patrolPoints;
    private NavMeshAgent _agent;
    private CharacterCombat _combat;
    private EnemyStats _stats;
    private CharacterAnimator _presentation;
    private Animator _animator;
    private CharacterStats _targetStats;
    private Transform _cachedTarget;
    private Enemy _settings;
    private NavMeshPath _path;
    private readonly Collider[] _candidates = new Collider[32];
    private readonly RaycastHit[] _sightHits = new RaycastHit[32];
    private Vector3 _home, _lastKnownPosition, _lastDestination, _progressPosition;
    private float _nextPathTime, _nextScanTime, _nextRecoveryTime, _progressTime;
    private float _lastSeenTime = float.NegativeInfinity;
    private float _hurtUntil;
    private int _patrolIndex;
    private bool _movementAllowed = true, _hasDestination, _holdingAttackRange, _hasBusy;
    private bool _hasSpeedPercent, _hasSpeed, _deathStarted;
    private float AttackRange => Mathf.Max(0.1f, _stats.range);
    private NavMeshQueryFilter Filter => new NavMeshQueryFilter { agentTypeID = _agent.agentTypeID, areaMask = _agent.areaMask };

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _combat = GetComponent<CharacterCombat>();
        _stats = GetComponent<EnemyStats>();
        _presentation = GetComponent<CharacterAnimator>();
        _animator = GetComponentInChildren<Animator>(true);
        _path = new NavMeshPath();
        if (_animator != null)
        {
            _animator.applyRootMotion = false;
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == "speedPercent") _hasSpeedPercent = true;
                if (parameter.name == "Speed") _hasSpeed = true;
                if (parameter.name == "isBusy") _hasBusy = true;
            }
        }
    }

    private void Start()
    {
        _settings = _stats.Definition;
        if (_settings == null) { enabled = false; return; }
        _home = _progressPosition = transform.position;
        _progressTime = Time.time;
        repathInterval = Mathf.Max(0.1f, _settings.repathInterval);
        patrolRadius = Mathf.Max(0f, _settings.patrolRadius);
        memoryDuration = Mathf.Max(0f, _settings.memoryDuration);
        turnSpeed = Mathf.Max(0f, _settings.turnSpeed);
        _agent.speed = Mathf.Max(0f, _stats.movementSpeed);
        _agent.acceleration = Mathf.Max(0.1f, _settings.acceleration);
        _agent.angularSpeed = turnSpeed;
        _agent.updatePosition = _agent.updateRotation = true;
        _agent.autoBraking = _agent.autoRepath = true;
        UpdateEquipment();
        RefreshTarget();
    }

    private void OnDisable()
    {
        Stop(true);
        if (_combat != null) _combat.CancelAttack();
        if (_presentation != null && !_deathStarted) _presentation.SetBusy(false);
    }

    private void Update()
    {
        if (_agent != null && _stats != null) _agent.speed = Mathf.Max(0f, _stats.movementSpeed * _stats.MovementMultiplier);
        if (_deathStarted || _settings == null || Time.timeScale == 0f) return;
        if (_stats.IsDead) { Die(); return; }
        RefreshTarget();
        if (Time.time >= _nextScanTime)
        {
            _nextScanTime = Time.time + 0.2f;
            if (!ValidTarget()) AcquirePlayer();
            if (ValidTarget() && CanSeeTarget())
            {
                _lastSeenTime = Time.time;
                _lastKnownPosition = target.position;
            }
        }
        bool remembers = ValidTarget() && Time.time - _lastSeenTime <= memoryDuration;
        bool busy = (_combat != null && _combat.IsExecutingAttack) || (_animator != null && _hasBusy && _animator.GetBool("isBusy"));
        if (Time.time < _hurtUntil)
        {
            CurrentState = State.TakingDamage;
            Stop();
        }
        else if (busy || !_movementAllowed)
        {
            if (CurrentState != State.TakingDamage) CurrentState = State.Attacking;
            Stop();
        }
        else if (remembers)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            bool visible = CanSeeTarget();
            if (visible && distance <= AttackRange) _holdingAttackRange = true;
            if (!visible || distance > AttackRange + _settings.attackRangeHysteresis) _holdingAttackRange = false;
            if (_holdingAttackRange)
            {
                CurrentState = State.PreparingAttack;
                Stop(true);
                FaceTarget();
                if (distance <= AttackRange && IsFacingTarget() && _combat != null && _combat.attackIsReady)
                {
                    _combat.Attack(false);
                    if (!_combat.attackIsReady) CurrentState = State.Attacking;
                }
            }
            else
            {
                CurrentState = visible ? State.InCombat : State.SearchingForTarget;
                MoveTo(_lastKnownPosition, Mathf.Max(0.1f, AttackRange * 0.8f));
            }
        }
        else
        {
            target = null;
            RefreshTarget();
            _holdingAttackRange = false;
            if (patrolEnabled) Patrol();
            else { CurrentState = State.Idle; Stop(true); }
        }
        if (_combat != null) _combat.InCombat = remembers;
        Animate();
    }

    private void RefreshTarget()
    {
        if (_cachedTarget == target) return;
        _cachedTarget = target;
        _targetStats = target != null ? target.GetComponentInParent<CharacterStats>() : null;
        _lastSeenTime = float.NegativeInfinity;
        _holdingAttackRange = false;
        _hasDestination = false;
    }

    private bool ValidTarget() => target != null && _targetStats != null && _targetStats.isActiveAndEnabled && !_targetStats.IsDead && _targetStats.currentHealth > 0 && !_targetStats.CompareTag(gameObject.tag);

    private void AcquirePlayer()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, _stats.sightRange, _candidates, whatIsPlayer, QueryTriggerInteraction.Ignore);
        Collider[] candidates = _candidates;
        if (count == _candidates.Length)
        {
            candidates = Physics.OverlapSphere(transform.position, _stats.sightRange, whatIsPlayer, QueryTriggerInteraction.Ignore);
            count = candidates.Length;
        }
        float best = float.PositiveInfinity;
        Transform selected = null;
        for (int i = 0; i < count; i++)
        {
            var player = candidates[i].GetComponentInParent<PlayerStats>();
            if (player == null || player.IsDead || !player.isActiveAndEnabled) continue;
            float distance = (player.transform.position - transform.position).sqrMagnitude;
            if (distance >= best) continue;
            target = player.transform;
            RefreshTarget();
            if (!CanSeeTarget()) continue;
            best = distance;
            selected = target;
        }
        target = selected;
        RefreshTarget();
    }

    public bool CanSeeTarget()
    {
        if (!isActiveAndEnabled || _settings == null || !ValidTarget()) return false;
        Vector3 delta = target.position - transform.position;
        if (delta.sqrMagnitude > _stats.sightRange * _stats.sightRange) return false;
        delta.y = 0f;
        if (delta.sqrMagnitude > 0.001f && Vector3.Angle(transform.forward, delta) > _settings.fieldOfView * 0.5f) return false;
        Vector3 origin = transform.position + Vector3.up;
        Vector3 destination = target.position + Vector3.up;
        Vector3 ray = destination - origin;
        int count = Physics.RaycastNonAlloc(origin, ray.normalized, _sightHits, ray.magnitude, ~ignoreLayers.value, QueryTriggerInteraction.Ignore);
        RaycastHit[] sightHits = _sightHits;
        if (count == _sightHits.Length)
        {
            sightHits = Physics.RaycastAll(origin, ray.normalized, ray.magnitude, ~ignoreLayers.value, QueryTriggerInteraction.Ignore);
            count = sightHits.Length;
        }
        for (int i = 0; i < count; i++)
        {
            Transform hit = sightHits[i].transform;
            if (hit.IsChildOf(transform) || hit.IsChildOf(_targetStats.transform)) continue;
            return false;
        }
        return true;
    }

    public bool IsFacingTarget()
    {
        if (target == null || _settings == null) return false;
        Vector3 delta = target.position - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude < 0.001f || Vector3.Angle(transform.forward, delta) <= _settings.attackFacingAngle;
    }

    private void FaceTarget()
    {
        Vector3 delta = target.position - transform.position;
        delta.y = 0f;
        if (delta.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta), turnSpeed * Time.deltaTime);
    }

    private bool EnsureOnNavMesh()
    {
        if (!_agent.enabled) return false;
        if (_agent.isOnNavMesh) return true;
        if (Time.time < _nextRecoveryTime) return false;
        _nextRecoveryTime = Time.time + 1f;
        return NavMesh.SamplePosition(transform.position, out NavMeshHit hit, _settings.navMeshSampleRadius, Filter) && _agent.Warp(hit.position);
    }

    private void MoveTo(Vector3 destination, float stoppingDistance)
    {
        if (!EnsureOnNavMesh()) return;
        _agent.stoppingDistance = stoppingDistance;
        bool invalid = !_agent.pathPending && (_agent.isPathStale || (_agent.hasPath && _agent.pathStatus != NavMeshPathStatus.PathComplete));
        if (Time.time >= _nextPathTime && (invalid || !_hasDestination || !_agent.hasPath || (destination - _lastDestination).sqrMagnitude >= _settings.destinationThreshold * _settings.destinationThreshold))
        {
            _nextPathTime = Time.time + repathInterval;
            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, _settings.navMeshSampleRadius, Filter) || !_agent.CalculatePath(hit.position, _path) || _path.status != NavMeshPathStatus.PathComplete)
            {
                Stop(true);
                return;
            }
            _agent.SetPath(_path);
            invalid = false;
            _lastDestination = destination;
            _hasDestination = true;
        }
        if (invalid) { Stop(true); return; }
        _agent.isStopped = false;
        if ((transform.position - _progressPosition).sqrMagnitude > 0.04f)
        {
            _progressPosition = transform.position;
            _progressTime = Time.time;
        }
        else if (!_agent.pathPending && _agent.hasPath && _agent.remainingDistance > stoppingDistance + 0.2f && Time.time - _progressTime > _settings.stuckTimeout)
        {
            Stop(true);
            _nextPathTime = Time.time + 1f;
            _progressTime = Time.time;
        }
        if (!_agent.pathPending) distanceToLastCorner = _agent.remainingDistance;
    }

    private void Patrol()
    {
        CurrentState = State.Patrol;
        if (!EnsureOnNavMesh()) return;
        bool arrived = !_agent.pathPending && (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance + 0.2f);
        if (arrived)
        {
            if (walkPointSet)
            {
                walkPointSet = false;
                Stop(true);
                _nextPathTime = Time.time + _settings.patrolWait;
            }
            if (Time.time < _nextPathTime) return;
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                var point = patrolPoints[_patrolIndex++ % patrolPoints.Length];
                if (point == null) { _nextPathTime = Time.time + repathInterval; return; }
                _lastKnownPosition = point.position;
            }
            else
            {
                Vector2 offset = Random.insideUnitCircle * patrolRadius;
                _lastKnownPosition = _home + new Vector3(offset.x, 0f, offset.y);
            }
            walkPointSet = true;
            _hasDestination = false;
        }
        MoveTo(_lastKnownPosition, 0.2f);
    }

    private void Stop(bool clear = false)
    {
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            if (clear) _agent.ResetPath();
        }
        if (clear) _hasDestination = false;
        _progressTime = Time.time;
        _progressPosition = transform.position;
    }

    private void Animate()
    {
        currentSpeed = _agent.enabled && _agent.isOnNavMesh ? _agent.velocity.magnitude : 0f;
        if (_animator == null) return;
        if (_hasSpeedPercent) _animator.SetFloat("speedPercent", _agent.speed > 0f ? currentSpeed / _agent.speed : 0f);
        if (_hasSpeed) _animator.SetFloat("Speed", currentSpeed);
    }

    public void NotifyDamaged(Transform attacker)
    {
        if (_deathStarted || attacker == null) return;
        var stats = attacker.GetComponentInParent<PlayerStats>();
        if (stats == null || stats.IsDead) return;
        target = stats.transform;
        RefreshTarget();
        _lastKnownPosition = target.position;
        _lastSeenTime = Time.time;
        _hurtUntil = Time.time + 0.3f;
        CurrentState = State.TakingDamage;
        Stop(true);
    }

    public void UpdateEquipment()
    {
        if (_agent != null && _stats != null) _agent.stoppingDistance = Mathf.Max(0.1f, AttackRange * 0.8f);
    }
    public void EnableMovement() => _movementAllowed = true;
    public void DisableMovement() { _movementAllowed = false; Stop(); }
    public void Die()
    {
        if (_deathStarted) return;
        _deathStarted = true;
        CurrentState = State.Dying;
        Stop(true);
        if (_combat != null)
        {
            _combat.canAttack = _combat.canBlock = _combat.attackIsReady = false;
            _combat.enabled = false;
        }
        if (_agent != null) _agent.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        var interaction = GetComponent<EnemyInteract>();
        if (interaction != null) { interaction.OnDefocused(); interaction.enabled = false; }
        Animate();
        if (_presentation != null) _presentation.Death();
        CurrentState = State.Dead;
        enabled = false;
        float lifetime = _stats.Definition != null ? _stats.Definition.corpseLifetime : 10f;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }
}
