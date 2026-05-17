using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that owns pawn registration and resource-click routing.
/// Attach to a persistent GameObject in the Game scene.
/// </summary>
public class PawnManager : MonoBehaviour
{
    public static PawnManager Instance { get; private set; }

    readonly List<Pawn> _pawns = new List<Pawn>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()  => ResourceNode.OnNodeClicked += HandleNodeClicked;
    void OnDisable() => ResourceNode.OnNodeClicked -= HandleNodeClicked;

    // ─── Registration ─────────────────────────────────────────────────────────

    public void RegisterPawn(Pawn p)
    {
        if (!_pawns.Contains(p)) _pawns.Add(p);
    }

    public void UnregisterPawn(Pawn p) => _pawns.Remove(p);

    // ─── Click Routing ────────────────────────────────────────────────────────

    void HandleNodeClicked(ResourceNode node)
    {
        if (node == null) return;

        if (node.AssignedPawn != null)
        {
            // Already assigned — cancel the task
            node.AssignedPawn.CancelTask();
        }
        else
        {
            // Find the nearest idle or wandering pawn
            var pawn = GetNearestAvailablePawn(node.transform.position);
            if (pawn != null)
                pawn.AssignToNode(node);
        }
    }

    Pawn GetNearestAvailablePawn(Vector3 pos)
    {
        Pawn best = null;
        float bestDist = float.MaxValue;

        foreach (var p in _pawns)
        {
            if (p == null) continue;
            if (p.State != PawnState.Idle && p.State != PawnState.Wandering) continue;

            float d = Vector3.Distance(p.transform.position, pos);
            if (d < bestDist) { bestDist = d; best = p; }
        }
        return best;
    }
}
