using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    [SerializeField] protected float maxHP = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float attackDamage = 10f;

    public float MoveSpeed    { get => moveSpeed;    set => moveSpeed    = value; }
    public float AttackDamage { get => attackDamage; set => attackDamage = value; }

    public event System.Action OnDied;

    public float CurrentHP { get; protected set; }
    public float MaxHP => maxHP;
    public Faction UnitFaction { get; protected set; }
    public bool IsDead { get; private set; }

    protected Animator        _animator;
    protected SpriteRenderer  _spriteRenderer;
    protected int             _waypointIndex;
    protected Transform[]     _waypoints;
    protected float           _damageMultiplier = 1f;

    // ── Defend mode (allied units from buildings) ─────────────────────────────
    protected bool    _isDefending;
    protected Vector3 _homePosition;
    protected float   _patrolRadius = 5f;
    protected Vector3 _pathOffset;       // random lateral scatter applied per-unit while marching
    protected Vector3 _wanderTarget;     // current wander destination near homePosition
    protected bool    _hasWanderTarget;  // false = pick a new wander spot next ReturnHome call

    public void InitForTest(float hp, Faction faction)
    {
        maxHP = hp; CurrentHP = hp; UnitFaction = faction;
    }

    // ── Optional HP override (called by TroopSpawner for allied unit boosting) ──
    public void SetMaxHP(float hp) { maxHP = hp; CurrentHP = hp; }

    /// <summary>Standard init: unit follows waypoints (used by WaveManager for enemies).</summary>
    public virtual void Initialize(Faction faction)
    {
        UnitFaction     = faction;
        CurrentHP       = maxHP;
        _waypoints      = PathManager.Instance?.GetWaypoints();
        _waypointIndex  = 0;
        // Prefer root-level Animator, fall back to children (handles nested sprite rigs)
        _animator       = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _isDefending    = false;
    }

    /// <summary>Defender init: unit stays near its building and attacks approaching enemies.</summary>
    public virtual void InitializeDefender(Faction faction, Vector3 homePosition, float patrolRadius = 5f)
    {
        Initialize(faction);
        _isDefending  = true;
        _homePosition = homePosition;
        _patrolRadius = patrolRadius;
        _waypoints    = null;   // defenders don't follow the enemy path
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        DamagePopup.Create(transform.position, amount);
        StartCoroutine(DamageFlash());
        if (CurrentHP <= 0f) Die();
    }

    IEnumerator DamageFlash()
    {
        if (_spriteRenderer == null) yield break;
        _spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
        yield return new WaitForSeconds(0.12f);
        if (_spriteRenderer != null) _spriteRenderer.color = Color.white;
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    public virtual void ApplyStatMultiplier(float mult)
    {
        maxHP             = maxHP * mult;
        CurrentHP         = maxHP;
        _damageMultiplier = mult;
    }

    protected virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDied?.Invoke();
        if (UnitFaction == FactionSelectUI.ChosenFaction)
            ResourceManager.Instance?.UnregisterTroop();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.unitDeath);
        ParticleSpawner.Spawn(ParticleSpawner.Dust, transform.position);
        if (Application.isPlaying) Destroy(gameObject, 0.1f);
    }

    // â”€â”€ Waypoint helpers (enemy march mode) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    protected Vector3 CurrentWaypoint =>
        _waypoints != null && _waypointIndex < _waypoints.Length
            ? _waypoints[_waypointIndex].position
            : transform.position;

    protected bool ReachedWaypoint() =>
        Vector3.Distance(transform.position, CurrentWaypoint + _pathOffset) < 0.4f;

    protected void AdvanceWaypoint()
    {
        if (_waypoints != null && _waypointIndex < _waypoints.Length - 1)
            _waypointIndex++;
    }

    protected void MoveTowardWaypoint()
    {
        Vector3 target = CurrentWaypoint + _pathOffset;
        Vector3 dir = (target - transform.position).normalized;
        Vector3 sep = GetSeparationForce();
        Vector3 move = dir.sqrMagnitude < 0.001f ? sep : (dir + sep * 0.4f).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(dir.x) > 0.01f)
            transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
    }

    public void SetPathOffset(Vector3 offset) => _pathOffset = offset;

    /// <summary>Move directly toward a world position (siege / chase).</summary>
    protected void MoveToward(Vector3 target)
    {
        Vector3 dir  = (target - transform.position).normalized;
        Vector3 sep  = GetSeparationForce();
        Vector3 move = (dir + sep * 0.35f).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(dir.x) > 0.01f)
            transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
    }

    /// <summary>
    /// Walk toward home, then wander lazily within ~2 units so troops don't all
    /// stack on the same pixel.  Plays runHash while moving, idleHash when resting.
    /// </summary>
    protected void ReturnHome(int runHash, int idleHash)
    {
        float distToHome = Vector3.Distance(transform.position, _homePosition);

        // If pushed too far away (e.g. after a fight), head straight home first.
        if (distToHome > 3f)
        {
            Vector3 dir  = (_homePosition - transform.position).normalized;
            Vector3 sep  = GetSeparationForce();
            Vector3 move = (dir + sep * 0.35f).normalized;
            transform.position += move * moveSpeed * Time.deltaTime;
            if (Mathf.Abs(dir.x) > 0.01f)
                transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
            _animator?.Play(runHash);
            _hasWanderTarget = false;
            return;
        }

        // Near home: pick a random wander spot and amble toward it.
        if (!_hasWanderTarget || Vector3.Distance(transform.position, _wanderTarget) < 0.3f)
        {
            _wanderTarget    = _homePosition + (Vector3)(Random.insideUnitCircle * 2f);
            _hasWanderTarget = true;
        }

        float wanderDist = Vector3.Distance(transform.position, _wanderTarget);
        if (wanderDist > 0.3f)
        {
            Vector3 dir  = (_wanderTarget - transform.position).normalized;
            Vector3 sep  = GetSeparationForce();
            Vector3 move = (dir + sep * 0.35f).normalized;
            transform.position += move * (moveSpeed * 0.5f) * Time.deltaTime;
            if (Mathf.Abs(dir.x) > 0.01f)
                transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
            _animator?.Play(runHash);
        }
        else
        {
            // Arrived at wander spot — rest briefly, then pick a new one next frame.
            _hasWanderTarget = false;
            _animator?.Play(idleHash);
        }
    }

    /// <summary>Override home position and patrol radius (called when spawning building is destroyed).</summary>
    public void SetDefendPosition(Vector3 newHome, float newRadius)
    {
        _homePosition = newHome;
        _patrolRadius = newRadius;
    }

    /// <summary>True once enemy marcher has reached (and stays at) the last waypoint.</summary>
    protected bool AtFinalWaypoint =>
        _waypoints != null && _waypoints.Length > 0
        && _waypointIndex >= _waypoints.Length - 1
        && ReachedWaypoint();

    // â”€â”€ Separation steering â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Returns a small repulsion vector away from same-faction allies that are
    /// too close, keeping units visually spread out without physics.
    /// </summary>
    protected Vector3 GetSeparationForce(float radius = 1.2f)
    {
        Vector3 force = Vector3.zero;
        int     count = 0;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            if (!h.TryGetComponent<Unit>(out var u)) continue;
            if (u.UnitFaction != UnitFaction) continue;   // only push away from allies
            Vector3 away = transform.position - h.transform.position;
            float   dist = away.magnitude;
            if (dist < 0.01f)
            {
                force += (Vector3)(Random.insideUnitCircle.normalized);
                count++;
                continue;
            }
            force += away.normalized * (radius - dist) / radius;
            count++;
        }
        return count > 0 ? force.normalized : Vector3.zero;
    }

    // â”€â”€ Targeting helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    protected Unit FindNearestEnemy(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        Unit  nearest = null;
        float minDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Unit>(out var u)) continue;
            if (u.UnitFaction == UnitFaction || u.IsDead) continue;
            if (u is PlayerWarrior) continue;   // enemies never target the player directly
            float d = Vector3.Distance(transform.position, u.transform.position);
            if (d < minDist) { minDist = d; nearest = u; }
        }
        return nearest;
    }

    /// <summary>Returns the nearest Building that belongs to the opposing faction.</summary>
    protected Building FindNearestEnemyBuilding(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        Building nearest = null;
        float    minDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Building>(out var b)) continue;
            if (b.Faction == UnitFaction) continue;
            if (b.CurrentHP <= 0f) continue;
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minDist) { minDist = d; nearest = b; }
        }
        return nearest;
    }
}
