using UnityEngine;

public class LancerAI : Unit
{
    [SerializeField] float attackRange = 0.8f;
    [SerializeField] float attackCooldown = 1.2f;

    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashGuard  = Animator.StringToHash("Guard");
    static readonly int HashRun    = Animator.StringToHash("Run");

    float _attackTimer;

    bool AtFinalWaypoint =>
        _waypoints != null && _waypoints.Length > 0
        && _waypointIndex >= _waypoints.Length - 1
        && ReachedWaypoint();

    void Update()
    {
        if (IsDead) return;
        Unit enemy = FindNearestEnemy(attackRange);
        if (enemy != null)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _animator?.Play(HashAttack);
                enemy.TakeDamage(attackDamage * _damageMultiplier);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
                ParticleSpawner.Spawn(ParticleSpawner.Dust, enemy.transform.position);
                _attackTimer = attackCooldown;
            }
        }
        else
        {
            Building bldg = FindNearestEnemyBuilding(attackRange * 2f);
            if (bldg != null)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _animator?.Play(HashAttack);
                    bldg.TakeDamage(attackDamage * _damageMultiplier);
                    AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
                    ParticleSpawner.Spawn(ParticleSpawner.Dust, bldg.transform.position);
                    _attackTimer = attackCooldown;
                }
            }
            else if (AtFinalWaypoint)
            {
                _animator?.Play(HashGuard);
            }
            else
            {
                if (ReachedWaypoint()) AdvanceWaypoint();
                MoveTowardWaypoint();
                _animator?.Play(HashRun);
            }
        }
    }
}
