using UnityEngine;

public class ArcherAI : Unit
{
    [SerializeField] float attackRange    = 5.0f;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float aggroRadius    = 6.0f;   // archers have longer sight while marching
    [SerializeField] GameObject arrowPrefab;

    static readonly int HashShoot = Animator.StringToHash("Shoot");
    static readonly int HashRun   = Animator.StringToHash("Run");
    static readonly int HashIdle  = Animator.StringToHash("Idle");

    float _attackTimer;
    float _attackAnimEndTime;

    // Target lock: once an enemy is engaged in defend mode, chase / shoot it until dead
    Unit _lockedTarget;

    void Update()
    {
        if (IsDead) return;

        if (_isDefending) { UpdateDefend(); return; }

        // ── Enemy march mode ──────────────────────────────────────────────────
        Unit enemy = FindNearestEnemy(aggroRadius);
        if (enemy != null)
        {
            FaceTarget(enemy.transform.position);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackAnimEndTime = Time.time + 0.5f;
                _animator?.Play(HashShoot);
                ShootAt(enemy);
                _attackTimer = attackCooldown;
            }
            else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
            return;
        }

        Building bldg = FindNearestEnemyBuilding(attackRange);
        if (bldg != null)
        {
            FaceTarget(bldg.transform.position);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackAnimEndTime = Time.time + 0.5f;
                _animator?.Play(HashShoot);
                ShootAt(bldg);
                _attackTimer = attackCooldown;
            }
            else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
            return;
        }

        if (AtFinalWaypoint)
        {
            Building siege = FindNearestEnemyBuilding(99f);
            if (siege != null) { MoveToward(siege.transform.position); _animator?.Play(HashRun); return; }
            _animator?.Play(HashIdle);
            return;
        }

        if (ReachedWaypoint()) AdvanceWaypoint();
        MoveTowardWaypoint();
        _animator?.Play(HashRun);
    }

    // ── Defend mode ───────────────────────────────────────────────────────────

    void UpdateDefend()
    {
        // ── Keep shooting locked target until it dies ────────────────────────
        if (_lockedTarget != null && !_lockedTarget.IsDead)
        {
            FaceTarget(_lockedTarget.transform.position);
            // Archers close the gap if target is too far even for arrows
            float dist = Vector3.Distance(transform.position, _lockedTarget.transform.position);
            if (dist > attackRange * 1.5f)
            {
                MoveToward(_lockedTarget.transform.position);
                _animator?.Play(HashRun);
                _attackTimer -= Time.deltaTime;
            }
            else
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _attackAnimEndTime = Time.time + 0.5f;
                    _animator?.Play(HashShoot);
                    ShootAt(_lockedTarget);
                    _attackTimer = attackCooldown;
                }
                else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
            }
            return;
        }
        _lockedTarget = null;

        // ── Scan for a new target ────────────────────────────────────────────
        Unit enemy = FindNearestEnemy(_patrolRadius);
        if (enemy != null)
        {
            _lockedTarget = enemy;
            FaceTarget(enemy.transform.position);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackAnimEndTime = Time.time + 0.5f;
                _animator?.Play(HashShoot);
                ShootAt(enemy);
                _attackTimer = attackCooldown;
            }
            else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
            return;
        }

        Building bldg = FindNearestEnemyBuilding(_patrolRadius);
        if (bldg != null)
        {
            FaceTarget(bldg.transform.position);
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackAnimEndTime = Time.time + 0.5f;
                _animator?.Play(HashShoot);
                ShootAt(bldg);
                _attackTimer = attackCooldown;
            }
            else if (Time.time >= _attackAnimEndTime) { _animator?.Play(HashIdle); }
            return;
        }

        ReturnHome(HashRun, HashIdle);
    }

    // ── Shoot helpers ─────────────────────────────────────────────────────────

    void ShootAt(Unit target)
    {
        if (arrowPrefab != null)
            Projectile.Fire(arrowPrefab, transform.position, target, attackDamage, _damageMultiplier);
        else
        {
            target.TakeDamage(attackDamage * _damageMultiplier);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
            ParticleSpawner.Spawn(ParticleSpawner.Dust, target.transform.position);
        }
    }

    void ShootAt(Building target)
    {
        if (arrowPrefab != null)
            Projectile.Fire(arrowPrefab, transform.position, target, attackDamage, _damageMultiplier);
        else
        {
            target.TakeDamage(attackDamage * _damageMultiplier);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
            ParticleSpawner.Spawn(ParticleSpawner.Dust, target.transform.position);
        }
    }

    void FaceTarget(Vector3 pos)
    {
        float dx = pos.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);
    }
}
