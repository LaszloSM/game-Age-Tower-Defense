using UnityEngine;

public class MonkAI : Unit
{
    [SerializeField] float healRadius    = 3.0f;
    [SerializeField] float healPerSecond = 10f;
    [SerializeField] ParticleSystem healEffect;

    static readonly int HashHeal = Animator.StringToHash("Heal");
    static readonly int HashRun  = Animator.StringToHash("Run");
    static readonly int HashIdle = Animator.StringToHash("Idle");

    void Update()
    {
        if (IsDead) return;

        bool moving = false;

        if (_isDefending)
        {
            if (Vector3.Distance(transform.position, _homePosition) > 1f)
            {
                MoveToward(_homePosition);
                moving = true;
            }
        }
        else
        {
            if (!AtFinalWaypoint)
            {
                if (ReachedWaypoint()) AdvanceWaypoint();
                MoveTowardWaypoint();
                moving = true;
            }
            else
            {
                Building siege = FindNearestEnemyBuilding(99f);
                if (siege != null) { MoveToward(siege.transform.position); moving = true; }
            }
        }

        if (moving) _animator?.Play(HashRun);
        HealNearby();
    }

    void HealNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, healRadius);
        bool healedAny = false;
        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Unit>(out var u)) continue;
            if (u.UnitFaction != UnitFaction || u.IsDead || u.CurrentHP >= u.MaxHP) continue;
            u.Heal(healPerSecond * Time.deltaTime);
            healedAny = true;
        }

        if (healedAny)
        {
            _animator?.Play(HashHeal);
            if (healEffect != null && !healEffect.isPlaying) healEffect.Play();
        }
        else
        {
            if (healEffect != null && healEffect.isPlaying) healEffect.Stop();
            bool moving = _isDefending
                ? Vector3.Distance(transform.position, _homePosition) > 1f
                : !AtFinalWaypoint;
            _animator?.Play(moving ? HashRun : HashIdle);
        }
    }
}
