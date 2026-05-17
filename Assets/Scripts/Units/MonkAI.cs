using UnityEngine;

public class MonkAI : Unit
{
    [SerializeField] float healRadius = 3.0f;
    [SerializeField] float healPerSecond = 10f;
    [SerializeField] ParticleSystem healEffect;

    static readonly int HashHeal = Animator.StringToHash("Heal");
    static readonly int HashRun  = Animator.StringToHash("Run");
    static readonly int HashIdle = Animator.StringToHash("Idle");

    void Update()
    {
        if (IsDead) return;
        if (!AtFinalWaypoint())
        {
            if (ReachedWaypoint()) AdvanceWaypoint();
            MoveTowardWaypoint();
        }
        HealNearby();
    }

    bool AtFinalWaypoint() =>
        _waypoints != null && _waypoints.Length > 0
        && _waypointIndex >= _waypoints.Length - 1
        && ReachedWaypoint();

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
            if (AtFinalWaypoint())
                _animator?.Play(HashIdle);
            else
                _animator?.Play(HashRun);
        }
    }
}
