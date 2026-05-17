using UnityEngine;

/// <summary>
/// A fired arrow / bolt.  Moves toward a target Unit or Building and deals
/// damage on arrival.  Spawned by ArcherAI.
///
/// Prefab setup:
///   – SpriteRenderer with the arrow sprite (rotated to face right by default)
///   – This script attached
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] float moveSpeed   = 10f;
    [SerializeField] float lifetime    = 4f;    // auto-destroy if it never reaches target

    float   _damage;
    float   _damageMultiplier = 1f;

    // Target can be a Unit OR a Building — only one is non-null at a time
    Unit     _targetUnit;
    Building _targetBuilding;

    Vector3 _lastKnownPos;   // used after target dies / is destroyed

    // ─── Factory methods ──────────────────────────────────────────────────────

    public static Projectile Fire(GameObject prefab, Vector3 from, Unit target, float damage, float mult = 1f)
    {
        if (prefab == null || target == null) return null;
        var go = Instantiate(prefab, from, Quaternion.identity);
        var p  = go.GetComponent<Projectile>();
        if (p != null) p.Init(target, damage, mult);
        return p;
    }

    public static Projectile Fire(GameObject prefab, Vector3 from, Building target, float damage, float mult = 1f)
    {
        if (prefab == null || target == null) return null;
        var go = Instantiate(prefab, from, Quaternion.identity);
        var p  = go.GetComponent<Projectile>();
        if (p != null) p.Init(target, damage, mult);
        return p;
    }

    void Init(Unit target, float dmg, float mult)
    {
        _targetUnit       = target;
        _damage           = dmg;
        _damageMultiplier = mult;
        _lastKnownPos     = target.transform.position;
        AimAt(_lastKnownPos);
        Destroy(gameObject, lifetime);
    }

    void Init(Building target, float dmg, float mult)
    {
        _targetBuilding   = target;
        _damage           = dmg;
        _damageMultiplier = mult;
        _lastKnownPos     = target.transform.position;
        AimAt(_lastKnownPos);
        Destroy(gameObject, lifetime);
    }

    // ─── Movement ─────────────────────────────────────────────────────────────

    void Update()
    {
        // Keep tracking a living unit; freeze on last-known pos if it died
        if (_targetUnit != null && !_targetUnit.IsDead)
            _lastKnownPos = _targetUnit.transform.position;
        else if (_targetBuilding != null && _targetBuilding.CurrentHP > 0f)
            _lastKnownPos = _targetBuilding.transform.position;

        AimAt(_lastKnownPos);

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, _lastKnownPos, step);

        if (Vector2.Distance(transform.position, _lastKnownPos) < 0.15f)
            Hit();
    }

    // ─── Impact ───────────────────────────────────────────────────────────────

    void Hit()
    {
        float finalDmg = _damage * _damageMultiplier;

        if (_targetUnit != null && !_targetUnit.IsDead)
        {
            _targetUnit.TakeDamage(finalDmg);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
        }
        else if (_targetBuilding != null && _targetBuilding.CurrentHP > 0f)
        {
            _targetBuilding.TakeDamage(finalDmg);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
        }

        Destroy(gameObject);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    void AimAt(Vector3 target)
    {
        Vector2 dir = (target - transform.position);
        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
