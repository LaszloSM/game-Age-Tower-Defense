using UnityEngine;

public class LancerAI : Unit
{
    [SerializeField] float attackRange    = 1.2f;
    [SerializeField] float attackCooldown = 1.2f;
    [SerializeField] float aggroRadius    = 3.5f;   // detection radius while marching

    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashGuard  = Animator.StringToHash("Guard");
    static readonly int HashRun    = Animator.StringToHash("Run");
    static readonly int HashIdle   = Animator.StringToHash("Idle");

    float _attackTimer;
    float _attackAnimEndTime;

    // Target lock: once an enemy is engaged in defend mode, chase it until dead
    Unit _lockedTarget;

    void Update()
    {
        if (IsDead) return;

        if (_isDefending) { UpdateDefend(); return; }

        // ── Enemy march mode ──────────────────────────────────────────────────
        Unit enemy = FindNearestEnemy(aggroRadius);
        if (enemy != null)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > attackRange) { MoveToward(enemy.transform.position); _animator?.Play(HashRun); _attackTimer -= Time.deltaTime; }
            else AttackUnit(enemy);
            return;
        }

        Building bldg = FindNearestEnemyBuilding(attackRange * 1.5f);
        if (bldg != null) { AttackBuilding(bldg); return; }

        if (AtFinalWaypoint)
        {
            Building siege = FindNearestEnemyBuilding(99f);
            if (siege != null) { MoveToward(siege.transform.position); _animator?.Play(HashRun); return; }
            _animator?.Play(HashGuard);
            return;
        }

        if (ReachedWaypoint()) AdvanceWaypoint();
        MoveTowardWaypoint();
        _animator?.Play(HashRun);
    }

    // ── Defend mode ───────────────────────────────────────────────────────────

    void UpdateDefend()
    {
        // ── Pursue locked target until it dies ───────────────────────────────
        if (_lockedTarget != null && !_lockedTarget.IsDead)
        {
            float dist = Vector3.Distance(transform.position, _lockedTarget.transform.position);
            if (dist > attackRange)
            {
                MoveToward(_lockedTarget.transform.position);
                _animator?.Play(HashRun);
                _attackTimer -= Time.deltaTime;
            }
            else
            {
                AttackUnit(_lockedTarget);
            }
            return;
        }
        _lockedTarget = null;

        // ── Scan for a new target ────────────────────────────────────────────
        Unit enemy = FindNearestEnemy(_patrolRadius);
        if (enemy != null)
        {
            _lockedTarget = enemy;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > attackRange)
            {
                MoveToward(enemy.transform.position);
                _animator?.Play(HashRun);
                _attackTimer -= Time.deltaTime;
            }
            else
            {
                AttackUnit(enemy);
            }
            return;
        }

        Building bldg = FindNearestEnemyBuilding(_patrolRadius);
        if (bldg != null)
        {
            float dist = Vector3.Distance(transform.position, bldg.transform.position);
            if (dist > attackRange) { MoveToward(bldg.transform.position); _animator?.Play(HashRun); }
            else AttackBuilding(bldg);
            return;
        }

        ReturnHome(HashRun, HashIdle);
    }

    // ── Attack helpers ────────────────────────────────────────────────────────

    void AttackUnit(Unit target)
    {
        FaceTarget(target.transform.position);
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackAnimEndTime = Time.time + 0.5f;
            _animator?.Play(HashAttack);
            target.TakeDamage(attackDamage * _damageMultiplier);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
            ParticleSpawner.Spawn(ParticleSpawner.Dust, target.transform.position);
            _attackTimer = attackCooldown;
        }
        else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
    }

    void AttackBuilding(Building target)
    {
        FaceTarget(target.transform.position);
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            _attackAnimEndTime = Time.time + 0.5f;
            _animator?.Play(HashAttack);
            target.TakeDamage(attackDamage * _damageMultiplier);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
            ParticleSpawner.Spawn(ParticleSpawner.Dust, target.transform.position);
            _attackTimer = attackCooldown;
        }
        else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
    }

    void FaceTarget(Vector3 pos)
    {
        float dx = pos.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);
    }
}
