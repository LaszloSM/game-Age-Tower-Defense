# Implementation of PAWN and Resource Systems

## 1. Create SheepAI.cs
```csharp
using UnityEngine;
using System.Collections;

public class SheepAI : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float wanderRadius = 3f;
    [SerializeField] float idleTime = 3f;
    [SerializeField] Animator animator;

    Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
        if (animator == null) animator = GetComponent<Animator>();
        StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (animator != null) animator.Play("Sheep_Idle");
            yield return new WaitForSeconds(Random.Range(idleTime * 0.5f, idleTime * 1.5f));

            Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
            Vector3 target = _startPos + new Vector3(randomDir.x, randomDir.y, 0);

            if (animator != null) animator.Play("Sheep_Run");
            
            while (Vector3.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                float dx = target.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.01f)
                    transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);
                yield return null;
            }
        }
    }
}
```

## 2. Update ResourceNode.cs
```csharp
using System.Collections;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] ResourceType type;
    [SerializeField] int maxAmount = 100;
    [SerializeField] int yieldPerTrip = 15;
    [SerializeField] float respawnSeconds = 90f;
    [SerializeField] Sprite depletedSprite;
    [SerializeField] Sprite fullSprite;

    [SerializeField] Animator animator;
    [SerializeField] string idleAnimationName;
    [SerializeField] string workAnimationName;

    SpriteRenderer _renderer;
    int _remaining;
    Pawn _assignedPawn;

    public ResourceType Type => type;
    public bool IsDepleted => _remaining <= 0;
    public bool HasPawn => _assignedPawn != null;

    public static event System.Action<ResourceNode> OnNodeClicked;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        _remaining = maxAmount;
    }

    void Start()
    {
        PlayIdleAnimation();
    }

    public void AssignPawn(Pawn pawn)   => _assignedPawn = pawn;
    public void UnassignPawn()          => _assignedPawn = null;

    public int Harvest()
    {
        if (IsDepleted) return 0;
        
        PlayWorkAnimation();

        int harvested = Mathf.Min(yieldPerTrip, _remaining);
        _remaining -= harvested;
        if (IsDepleted) StartCoroutine(RespawnRoutine());
        RefreshSprite();
        return harvested;
    }

    public void PlayIdleAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            animator.Play(idleAnimationName);
        }
    }

    public void PlayWorkAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(workAnimationName))
        {
            animator.Play(workAnimationName);
        }
    }

    void RefreshSprite()
    {
        if (_renderer == null) return;
        _renderer.sprite = IsDepleted ? depletedSprite : fullSprite;
    }

    IEnumerator RespawnRoutine()
    {
        _assignedPawn?.OnNodeDepleted();
        yield return new WaitForSeconds(respawnSeconds);
        _remaining = maxAmount;
        RefreshSprite();
        PlayIdleAnimation();
    }

    void OnMouseDown() => OnNodeClicked?.Invoke(this);
}
```

## 3. Update Pawn.cs
```csharp
using UnityEngine;
using System.Collections;

public enum PawnState { Idle, MovingToNode, Working, MovingBack, Depositing, Repairing }

public class Pawn : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float workDuration = 2f;
    [SerializeField] Animator animator;

    public PawnState State { get; private set; } = PawnState.Idle;
    public Faction PawnFaction { get; set; }

    ResourceNode _targetNode;
    Building _targetBuilding;
    int _carryAmount;
    Transform _castleTransform;

    void Start()
    {
        var castle = FindFirstObjectByType<Castle>();
        if (castle != null) _castleTransform = castle.transform;
        ResourceManager.Instance?.RegisterPawn();
        PlayAnim("Idle");
    }

    void OnDestroy() => ResourceManager.Instance?.UnregisterPawn();

    public void AssignToNode(ResourceNode node)
    {
        StopAllCoroutines();
        _targetNode = node;
        node?.AssignPawn(this);
        State = PawnState.MovingToNode;
        StartCoroutine(GatherRoutine());
    }

    public void AssignToRepair(Building building)
    {
        StopAllCoroutines();
        _targetBuilding = building;
        State = PawnState.Repairing;
        StartCoroutine(RepairRoutine());
    }

    public void OnNodeDepleted()
    {
        StopAllCoroutines();
        _targetNode = null;
        State = PawnState.Idle;
        PlayAnim("Idle");
    }

    IEnumerator GatherRoutine()
    {
        if (_targetNode == null) { State = PawnState.Idle; yield break; }
        
        string tool = GetToolForResource(_targetNode.Type);
        PlayAnim("Run", tool);
        yield return MoveTo(_targetNode.transform.position);

        State = PawnState.Working;
        PlayAnim("Interact", tool);
        yield return new WaitForSeconds(workDuration);

        _carryAmount = _targetNode.Harvest();

        State = PawnState.MovingBack;
        string resource = GetResourceTool(_targetNode.Type);
        PlayAnim("Run", resource);
        
        Vector3 deposit = _castleTransform != null ? _castleTransform.position : Vector3.zero;
        yield return MoveTo(deposit);

        State = PawnState.Depositing;
        ResourceManager.Instance?.AddResource(_targetNode.Type, _carryAmount);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.resourceCollect);
        _carryAmount = 0;
        
        State = PawnState.Idle;
        PlayAnim("Idle");

        if (_targetNode != null && !_targetNode.IsDepleted)
            AssignToNode(_targetNode);
    }

    IEnumerator RepairRoutine()
    {
        if (_targetBuilding == null) { State = PawnState.Idle; yield break; }
        
        PlayAnim("Run", "Hammer");
        yield return MoveTo(_targetBuilding.transform.position);
        
        PlayAnim("Interact", "Hammer");
        while (_targetBuilding != null && !_targetBuilding.IsFullHealth)
        {
            _targetBuilding.Repair(5f * Time.deltaTime);
            yield return null;
        }
        
        State = PawnState.Idle;
        PlayAnim("Idle");
    }

    IEnumerator MoveTo(Vector3 dest)
    {
        while (Vector3.Distance(transform.position, dest) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime);
            float dx = dest.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.01f)
                transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);
            yield return null;
        }
    }

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
        ResourceType.Wood => "Axe",
        ResourceType.Gold => "Pickaxe",
        ResourceType.Food => "Knife",
        _ => ""
    };

    string GetResourceTool(ResourceType type) => type switch
    {
        ResourceType.Wood => "Wood",
        ResourceType.Gold => "Gold",
        ResourceType.Food => "Meat",
        _ => ""
    };
}
```

## 4. Prefab Configuration (Scripted)
- Add `SheepAI` to `SheepNode`.
- Set Sorting Order = 10 for `BluePawn` and `RedPawn`.
- Set Animation names for all Nodes.
