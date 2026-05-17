using UnityEngine;

public class WarriorAI : Unit
{
    [SerializeField] float attackRange = 0.8f;
    [SerializeField] float attackCooldown = 1f;

    static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    static readonly int HashAttack2 = Animator.StringToHash("Attack2");
    static readonly int HashRun     = Animator.StringToHash("Run");

    bool _attackToggle;
    float _attackTimer;

    void Update()
    {
        if (IsDead) return;
        Unit enemy = FindNearestEnemy(attackRange);
        if (enemy != null)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackToggle = !_attackToggle;
                _animator?.Play(_attackToggle ? HashAttack1 : HashAttack2);
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
                // Stand still and beat on the building
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _attackToggle = !_attackToggle;
                    _animator?.Play(_attackToggle ? HashAttack1 : HashAttack2);
                    bldg.TakeDamage(attackDamage * _damageMultiplier);
                    AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
                    ParticleSpawner.Spawn(ParticleSpawner.Dust, bldg.transform.position);
                    _attackTimer = attackCooldown;
                }
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
