using UnityEngine;

public class PlayerWarrior : MonoBehaviour
{
    [SerializeField] float moveSpeed = 15f;
    [SerializeField] float attackDuration = 0.4f;

    static readonly int HashIdle    = Animator.StringToHash("Idle");
    static readonly int HashRun     = Animator.StringToHash("Run");
    static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    static readonly int HashAttack2 = Animator.StringToHash("Attack2");

    Animator       _animator;
    SpriteRenderer _sr;
    bool           _attacking;
    float          _attackTimer;
    bool           _attackToggle;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sr       = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Attack input
        if (Input.GetMouseButtonDown(0) && !_attacking)
        {
            _attacking = true;
            _attackTimer = attackDuration;
            _attackToggle = !_attackToggle;
            _animator?.Play(_attackToggle ? HashAttack1 : HashAttack2);
        }

        if (_attacking)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f) _attacking = false;
            return;
        }

        // Movement
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        var dir = new Vector3(h, v, 0f).normalized;

        transform.position += dir * moveSpeed * Time.deltaTime;

        bool moving = dir.sqrMagnitude > 0.01f;
        _animator?.Play(moving ? HashRun : HashIdle);

        if (moving && Mathf.Abs(h) > 0.01f)
            _sr.flipX = h < 0f;
    }
}
