using System.Collections;
using UnityEngine;

/// <summary>
/// Resource node on the map. Supports three depletion behaviours:
///   Stay    – tree/gold: stay in place, show depleted visual (stump / empty rock)
///   Wander  – sheep: death flash then respawn at a random nearby position
/// </summary>
public class ResourceNode : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [SerializeField] ResourceType type;
    [SerializeField] int   maxAmount     = 100;
    [SerializeField] int   yieldPerTrip  = 15;
    [SerializeField] float respawnSeconds = 30f;

    [Header("Visuals")]
    [SerializeField] Sprite depletedSprite;
    [SerializeField] Sprite fullSprite;
    [SerializeField] Animator animator;
    [SerializeField] string idleAnimationName     = "Idle";
    [SerializeField] string depletedAnimationName = "";

    [Header("Pawn Work Position")]
    [Tooltip("How many world units from this node's center the pawn stands to work. " +
             "Decrease to get closer, increase to stand farther. " +
             "Typical values: trees 3-4, gold 2-3, sheep 1.5-2.5")]
    [SerializeField] float workStopDistance = 3f;

    [Header("Sheep – random respawn")]
    [Tooltip("If true the node plays a death effect then reappears at a random offset position.")]
    [SerializeField] bool  wanderOnDeplete = false;
    [SerializeField] float wanderRadiusMin = 5f;
    [SerializeField] float wanderRadiusMax = 15f;

    // ─── Runtime ─────────────────────────────────────────────────────────────
    SpriteRenderer _renderer;
    Collider2D     _collider;
    int            _remaining;
    Pawn           _assignedPawn;
    bool           _isRespawning;
    Vector3        _originalPosition;

    // ─── Properties ──────────────────────────────────────────────────────────
    public ResourceType Type       => type;
    public bool IsDepleted         => _remaining <= 0;
    public bool IsRespawning       => _isRespawning;
    public Pawn AssignedPawn       => _assignedPawn;
    public float WorkStopDistance  => workStopDistance;

    public static event System.Action<ResourceNode> OnNodeClicked;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    void Awake()
    {
        _renderer         = GetComponent<SpriteRenderer>();
        _collider         = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();
        _remaining        = maxAmount;
        _originalPosition = transform.position;
    }

    void Start() => RefreshVisuals();

    // ─── Assignment ───────────────────────────────────────────────────────────
    public void AssignPawn(Pawn pawn) => _assignedPawn = pawn;
    public void ClearAssignedPawn()   => _assignedPawn = null;

    // ─── Harvesting ───────────────────────────────────────────────────────────
    public int Harvest()
    {
        if (IsDepleted || _isRespawning) return 0;

        int harvested  = Mathf.Min(yieldPerTrip, _remaining);
        _remaining    -= harvested;

        if (IsDepleted)
            StartCoroutine(DepleteRoutine());
        else
            RefreshVisuals();

        return harvested;
    }

    // ─── Depletion / Respawn ─────────────────────────────────────────────────
    IEnumerator DepleteRoutine()
    {
        _isRespawning = true;
        if (_collider != null) _collider.enabled = false;

        // Wait one frame so Harvest()'s call stack fully unwinds before notifying the pawn.
        // This ensures the pawn's GatherRoutine can set State=MovingBack before OnNodeDepleted runs.
        yield return null;

        // Notify the assigned pawn
        if (_assignedPawn != null)
        {
            _assignedPawn.OnNodeDepleted();
            _assignedPawn = null;
        }

        if (wanderOnDeplete)
        {
            // ── Sheep death effect: red flash + shake ──────────────────────
            yield return StartCoroutine(SheepDeathEffect());
            if (_renderer != null) _renderer.enabled = false;
        }
        else
        {
            // Tree / Gold: show depleted visual immediately
            if (!string.IsNullOrEmpty(depletedAnimationName) && animator != null)
                animator.Play(depletedAnimationName);
            else if (depletedSprite != null && _renderer != null)
                _renderer.sprite = depletedSprite;
            else if (_renderer != null)
                _renderer.enabled = false;
        }

        yield return new WaitForSeconds(respawnSeconds);

        // ── Respawn ──────────────────────────────────────────────────────────
        if (wanderOnDeplete)
        {
            Vector2 dir    = Random.insideUnitCircle.normalized;
            float   dist   = Random.Range(wanderRadiusMin, wanderRadiusMax);
            transform.position = _originalPosition + new Vector3(dir.x * dist, dir.y * dist, 0f);
            _originalPosition  = transform.position;
        }

        _remaining = maxAmount;
        _isRespawning = false;
        if (_collider != null) _collider.enabled = true;
        if (_renderer != null) _renderer.enabled = true;
        RefreshVisuals();
    }

    // ─── Sheep death effect ───────────────────────────────────────────────────
    IEnumerator SheepDeathEffect()
    {
        if (_renderer == null) yield break;

        float elapsed = 0f;
        float duration = 0.8f;
        Vector3 basePos   = transform.localPosition;
        Vector3 baseScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Flash: alternate red tint and white 4 times
            float flash = Mathf.Sin(t * Mathf.PI * 8f);
            _renderer.color = flash > 0
                ? new Color(1f, 0.15f, 0.15f, 1f)   // red hit
                : Color.white;

            // Shake: random horizontal jitter that decreases over time
            float shake = (1f - t) * 0.4f;
            transform.localPosition = basePos + new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake * 0.5f, shake * 0.5f), 0f);

            // Squash then stretch
            float sq = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
            transform.localScale = new Vector3(baseScale.x * sq, baseScale.y / sq, 1f);

            yield return null;
        }

        // Reset transform
        transform.localPosition = basePos;
        transform.localScale    = baseScale;
        _renderer.color         = Color.white;
    }

    // ─── Visuals ──────────────────────────────────────────────────────────────
    void RefreshVisuals()
    {
        if (_renderer != null)
        {
            if (IsDepleted && depletedSprite != null)
                _renderer.sprite = depletedSprite;
            else if (!IsDepleted && fullSprite != null)
                _renderer.sprite = fullSprite;
        }

        if (!IsDepleted && !_isRespawning)
            PlayIdleAnimation();
    }

    public void PlayIdleAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(idleAnimationName))
            animator.Play(idleAnimationName);
    }

    // ─── Mouse Events ─────────────────────────────────────────────────────────
    void OnMouseDown()
    {
        if (!IsDepleted && !_isRespawning)
            OnNodeClicked?.Invoke(this);
    }

    void OnMouseEnter()
    {
        if (IsDepleted || _isRespawning) return;
        if (_assignedPawn != null)
            CursorManager.Instance?.SetCancel();
        else
            CursorManager.Instance?.SetInteract();
    }

    void OnMouseExit() => CursorManager.Instance?.SetDefault();
}
