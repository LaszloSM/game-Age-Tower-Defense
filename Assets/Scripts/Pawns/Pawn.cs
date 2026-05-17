using UnityEngine;
using System.Collections;

public enum PawnState { Idle, Wandering, MovingToNode, Working, MovingBack, Depositing, Repairing }

public class Pawn : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float workDuration = 2f;
    [SerializeField] float wanderRadius = 8f;
    [SerializeField] float wanderWaitMin = 1f;
    [SerializeField] float wanderWaitMax = 3f;
    [SerializeField] Animator animator;

    public PawnState State { get; private set; } = PawnState.Idle;
    public Faction PawnFaction { get; set; }

    ResourceNode _targetNode;
    Building _targetBuilding;
    int _carryAmount;
    Transform _castleTransform;
    Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        var castle = FindFirstObjectByType<Castle>();
        if (castle != null) _castleTransform = castle.transform;

        ResourceManager.Instance?.RegisterPawn();
        PawnManager.Instance?.RegisterPawn(this);

        if (animator == null) animator = GetComponent<Animator>();

        PlayAnim("Idle");
        StartCoroutine(WanderRoutine());
    }

    void OnDestroy()
    {
        ResourceManager.Instance?.UnregisterPawn();
        PawnManager.Instance?.UnregisterPawn(this);
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void AssignToNode(ResourceNode node)
    {
        if (node == null) return;
        StopAllCoroutines();
        _targetNode = node;
        node.AssignPawn(this);
        State = PawnState.MovingToNode;
        StartCoroutine(GatherRoutine());
    }

    public void AssignToRepair(Building building)
    {
        if (building == null) return;
        StopAllCoroutines();
        _targetBuilding = building;
        State = PawnState.Repairing;
        StartCoroutine(RepairRoutine());
    }

    /// <summary>Called by PawnManager when the player cancels a node assignment.</summary>
    public void CancelTask()
    {
        StopAllCoroutines();
        if (_targetNode != null)
        {
            _targetNode.ClearAssignedPawn();
            _targetNode = null;
        }
        State = PawnState.Idle;
        PlayAnim("Idle");
        StartCoroutine(WanderRoutine());
    }

    /// <summary>Called by ResourceNode when it becomes depleted mid-trip.</summary>
    public void OnNodeDepleted()
    {
        _targetNode = null;

        // If the pawn already harvested and is heading back, let it finish depositing.
        if (State == PawnState.MovingBack || State == PawnState.Depositing)
            return;

        StopAllCoroutines();
        State = PawnState.Idle;
        PlayAnim("Idle");
        StartCoroutine(WanderRoutine());
    }

    // ─── Wandering ────────────────────────────────────────────────────────────

    IEnumerator WanderRoutine()
    {
        State = PawnState.Wandering;
        while (State == PawnState.Wandering)
        {
            Vector3 center = _castleTransform != null ? _castleTransform.position : transform.position;

            // Wander at least 40% of radius so pawns don't cluster at the castle center
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist  = Random.Range(wanderRadius * 0.4f, wanderRadius);
            Vector3 dest = new Vector3(
                center.x + Mathf.Cos(angle) * dist,
                center.y + Mathf.Sin(angle) * dist,
                transform.position.z);

            PlayAnim("Run");
            yield return MoveTo(dest);

            PlayAnim("Idle");
            yield return new WaitForSeconds(Random.Range(wanderWaitMin, wanderWaitMax));
        }
    }

    // ─── Gathering ────────────────────────────────────────────────────────────

    IEnumerator GatherRoutine()
    {
        if (_targetNode == null) { State = PawnState.Idle; StartCoroutine(WanderRoutine()); yield break; }

        string tool = GetToolForResource(_targetNode.Type);

        // Stand at the EDGE of the resource collider, not its center
        Vector3 gatherPos = GetClosestPointOnNode(_targetNode);

        PlayAnim("Run", tool);
        yield return MoveTo(gatherPos, 0.25f);

        State = PawnState.Working;
        PlayAnim("Interact", tool);
        yield return new WaitForSeconds(workDuration);

        // Capture type NOW before Harvest() can synchronously null _targetNode via OnNodeDepleted
        ResourceType harvestedType = _targetNode != null ? _targetNode.Type : ResourceType.Wood;

        if (_targetNode != null)
            _carryAmount = _targetNode.Harvest();

        State = PawnState.MovingBack;
        PlayAnim("Run", GetResourceTool(harvestedType));

        // Deposit at the castle entrance, not deep inside
        Vector3 deposit = GetCastleDepositPoint();
        yield return MoveTo(deposit, 1.5f);

        State = PawnState.Depositing;
        if (_carryAmount > 0)
            ResourceManager.Instance?.AddResource(harvestedType, _carryAmount);

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.resourceCollect);
        _carryAmount = 0;

        State = PawnState.Idle;
        PlayAnim("Idle");

        // Loop back to node if it still exists and hasn't been depleted
        if (_targetNode != null && !_targetNode.IsDepleted && !_targetNode.IsRespawning)
            AssignToNode(_targetNode);
        else
        {
            _targetNode = null;
            State = PawnState.Idle;
            StartCoroutine(WanderRoutine());
        }
    }

    // ─── Repairing ────────────────────────────────────────────────────────────

    IEnumerator RepairRoutine()
    {
        if (_targetBuilding == null) { State = PawnState.Idle; StartCoroutine(WanderRoutine()); yield break; }

        Vector3 repairPos = GetClosestPointOnBuilding(_targetBuilding);
        PlayAnim("Run", "Hammer");
        yield return MoveTo(repairPos, 0.4f);

        PlayAnim("Interact", "Hammer");
        while (_targetBuilding != null && !_targetBuilding.IsFullHealth)
        {
            _targetBuilding.Repair(5f * Time.deltaTime);
            yield return null;
        }

        State = PawnState.Idle;
        PlayAnim("Idle");
        StartCoroutine(WanderRoutine());
    }

    // ─── Movement ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Moves the pawn toward dest.  Uses Vector2.MoveTowards so it never overshoots
    /// the destination (prevents the rapid back-and-forth oscillation bug).
    /// Reads _rb.position after each step so distance checks stay in sync with physics.
    /// </summary>
    IEnumerator MoveTo(Vector3 dest, float stopDist = 0.3f)
    {
        Vector2 dest2D = new Vector2(dest.x, dest.y);

        while (true)
        {
            // Always read the authoritative physics position, not transform.position
            Vector2 currentPos = _rb != null ? _rb.position : (Vector2)transform.position;

            float remaining = Vector2.Distance(currentPos, dest2D);
            if (remaining <= stopDist) break;

            // MoveTowards clamps the step — can never overshoot, no oscillation
            float step     = moveSpeed * Time.fixedDeltaTime;
            Vector2 newPos = Vector2.MoveTowards(currentPos, dest2D, step);

            if (_rb != null)
                _rb.MovePosition(newPos);
            else
                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            // Flip sprite to face movement direction
            float dx = dest2D.x - currentPos.x;
            if (Mathf.Abs(dx) > 0.05f)
                transform.localScale = new Vector3(dx < 0 ? -1f : 1f, 1f, 1f);

            yield return new WaitForFixedUpdate();
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the work position: a point exactly node.WorkStopDistance units from the
    /// node center, in the direction the pawn is approaching from.
    /// Tune WorkStopDistance on each ResourceNode in the Inspector.
    /// </summary>
    Vector3 GetClosestPointOnNode(ResourceNode node)
    {
        Vector2 dir = ((Vector2)transform.position - (Vector2)node.transform.position);
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;
        dir.Normalize();
        float radius = node.WorkStopDistance;
        return node.transform.position + (Vector3)(dir * radius);
    }

    /// <summary>Returns the point just outside a building's collider surface closest to this pawn.</summary>
    Vector3 GetClosestPointOnBuilding(Building building)
    {
        var col = building.GetComponent<Collider2D>();
        if (col == null) return building.transform.position;
        Vector2 surface = Physics2D.ClosestPoint(transform.position, col);
        Vector2 outDir  = (surface - (Vector2)building.transform.position);
        if (outDir.sqrMagnitude < 0.001f) outDir = Vector2.down;
        outDir.Normalize();
        Vector2 workPos = surface + outDir * 0.6f;
        return new Vector3(workPos.x, workPos.y, transform.position.z);
    }

    /// <summary>Returns the deposit point just at the castle entrance (bottom of castle).</summary>
    Vector3 GetCastleDepositPoint()
    {
        if (_castleTransform == null) return Vector3.zero;
        // Place pawn at the castle entrance: slightly in front of (below) the castle center
        return _castleTransform.position + Vector3.down * 5f;
    }

    // ─── Animation ────────────────────────────────────────────────────────────

    void PlayAnim(string stateName, string tool = "")
    {
        if (animator == null) return;
        string faction = PawnFaction.ToString();
        string finalName = string.IsNullOrEmpty(tool)
            ? $"Pawn_{stateName}_{faction}"
            : $"Pawn_{stateName} {tool}_{faction}";
        animator.Play(finalName);
    }

    string GetToolForResource(ResourceType type) => type switch
    {
        ResourceType.Wood  => "Axe",
        ResourceType.Gold  => "Pickaxe",
        ResourceType.Food  => "Knife",
        _                  => ""
    };

    string GetResourceTool(ResourceType type) => type switch
    {
        ResourceType.Wood  => "Wood",
        ResourceType.Gold  => "Gold",
        ResourceType.Food  => "Meat",
        _                  => ""
    };
}
