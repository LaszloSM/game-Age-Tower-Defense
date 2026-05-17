using UnityEngine;

public class ArcherAI : Unit
{
    [SerializeField] float attackRange = 5.0f;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] GameObject arrowPrefab;   // assign Arrow prefab in Inspector (optional)

    static readonly int HashShoot = Animator.StringToHash("Shoot");
    static readonly int HashRun   = Animator.StringToHash("Run");
    static readonly int HashIdle  = Animator.StringToHash("Idle");

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
                _animator?.Play(HashShoot);
                ShootAt(enemy);
                _attackTimer = attackCooldown;
            }
            else
            {
                _animator?.Play(HashIdle);
            }
        }
        else
        {
            Building bldg = FindNearestEnemyBuilding(attackRange);
            if (bldg != null)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _animator?.Play(HashShoot);
                    ShootAt(bldg);
                    _attackTimer = attackCooldown;
                }
                else
                {
                    _animator?.Play(HashIdle);
                }
            }
            else if (ReachedWaypoint())
            {
                AdvanceWaypoint();
                _animator?.Play(HashIdle);
            }
            else
            {
                MoveTowardWaypoint();
                _animator?.Play(HashRun);
            }
        }
    }

    void ShootAt(Unit target)
    {
        // Flip sprite toward target
        float dx = target.transform.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);

        if (arrowPrefab != null)
            Projectile.Fire(arrowPrefab, transform.position, target, attackDamage, _damageMultiplier);
        else
        {
            // Fallback: instant hit if no arrow prefab assigned
            target.TakeDamage(attackDamage * _damageMultiplier);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
        }
    }

    void ShootAt(Building target)
    {
        float dx = target.transform.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);

        if (arrowPrefab != null)
            Projectile.Fire(arrowPrefab, transform.position, target, attackDamage, _damageMultiplier);
        else
        {
            target.TakeDamage(attackDamage * _damageMultiplier);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackHit);
        }
    }
}
