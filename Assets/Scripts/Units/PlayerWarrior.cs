using UnityEngine;

/// <summary>
/// Player-controlled warrior. Extends Unit so the HP / damage-flash
/// infrastructure is shared. Enemies never directly target the player
/// (FindNearestEnemy skips PlayerWarrior instances).
/// </summary>
public class PlayerWarrior : Unit
{
    [Header("Player Settings")]
    [SerializeField] float playerMoveSpeed = 15f;
    [SerializeField] float attackDuration  = 0.4f;
    [SerializeField] float meleeRadius     = 2.5f;   // wider swing → fewer misses

    static readonly int HashIdle    = Animator.StringToHash("Idle");
    static readonly int HashRun     = Animator.StringToHash("Run");
    static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    static readonly int HashAttack2 = Animator.StringToHash("Attack2");

    SpriteRenderer _sr;
    bool           _attacking;
    float          _attackTimer;
    bool           _attackToggle;

    // ── Lifecycle ───────────────────────────────────────────────────────────

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Initialize(FactionSelectUI.ChosenFaction);
        moveSpeed = playerMoveSpeed;   // override Unit default (2 f)
    }

    // ── Update ───────────────────────────────────────────────────────────────

    void Update()
    {
        if (IsDead) return;

        // ── Attack input (V key) ─────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.V) && !_attacking)
        {
            _attacking    = true;
            _attackTimer  = attackDuration;
            _attackToggle = !_attackToggle;
            _animator?.Play(_attackToggle ? HashAttack1 : HashAttack2);
            DealMeleeHits();
        }

        // Count down attack timer (but DON'T return — let movement run simultaneously)
        if (_attacking)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f) _attacking = false;
        }

        // ── Movement (always active, even mid-swing) ─────────────────────────
        float h   = Input.GetAxisRaw("Horizontal");
        float v   = Input.GetAxisRaw("Vertical");
        var   dir = new Vector3(h, v, 0f).normalized;

        transform.position += dir * moveSpeed * Time.deltaTime;

        // Update movement animation only when not playing an attack
        if (!_attacking)
        {
            bool moving = dir.sqrMagnitude > 0.01f;
            _animator?.Play(moving ? HashRun : HashIdle);
        }

        // Flip sprite to face movement direction
        if (dir.sqrMagnitude > 0.01f && Mathf.Abs(h) > 0.01f)
            _sr.flipX = h < 0f;
    }

    // ── Melee swing ──────────────────────────────────────────────────────────

    void DealMeleeHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRadius);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<Unit>(out var u) && u != this
                && u.UnitFaction != UnitFaction && !u.IsDead)
            {
                u.TakeDamage(attackDamage * _damageMultiplier);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
            }
            else if (h.TryGetComponent<Building>(out var b) && b.Faction != UnitFaction)
            {
                b.TakeDamage(attackDamage * _damageMultiplier);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
            }
        }
    }

    // ── Death ────────────────────────────────────────────────────────────────

    protected override void Die()
    {
        if (IsDead) return;
        base.Die();
        GameManager.Instance?.ChangeState(GameState.GameOver);
    }
}
