using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] protected float maxHP = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float attackDamage = 10f;

    public event System.Action OnDied;

    public float CurrentHP { get; protected set; }
    public float MaxHP => maxHP;
    public Faction UnitFaction { get; protected set; }
    public bool IsDead { get; private set; }

    protected Animator _animator;
    protected int _waypointIndex;
    protected Transform[] _waypoints;
    protected float _damageMultiplier = 1f;

    public void InitForTest(float hp, Faction faction)
    {
        maxHP = hp; CurrentHP = hp; UnitFaction = faction;
    }

    public virtual void Initialize(Faction faction)
    {
        UnitFaction = faction;
        CurrentHP = maxHP;
        _waypoints = PathManager.Instance?.GetWaypoints();
        _waypointIndex = 0;
        _animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        if (CurrentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    public virtual void ApplyStatMultiplier(float mult)
    {
        maxHP = maxHP * mult;
        CurrentHP = maxHP;
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

    protected Vector3 CurrentWaypoint =>
        _waypoints != null && _waypointIndex < _waypoints.Length
            ? _waypoints[_waypointIndex].position
            : transform.position;

    protected bool ReachedWaypoint() =>
        Vector3.Distance(transform.position, CurrentWaypoint) < 0.15f;

    protected void AdvanceWaypoint()
    {
        if (_waypoints != null && _waypointIndex < _waypoints.Length - 1)
            _waypointIndex++;
    }

    protected void MoveTowardWaypoint()
    {
        Vector3 dir = (CurrentWaypoint - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(dir.x) > 0.01f)
            transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
    }

    protected Unit FindNearestEnemy(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        Unit nearest = null;
        float minDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Unit>(out var u)) continue;
            if (u.UnitFaction == UnitFaction || u.IsDead) continue;
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
        float minDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Building>(out var b)) continue;
            if (b.Faction == UnitFaction) continue;   // same faction → skip
            if (b.CurrentHP <= 0f) continue;
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minDist) { minDist = d; nearest = b; }
        }
        return nearest;
    }
}
