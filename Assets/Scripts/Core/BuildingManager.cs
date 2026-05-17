using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }
    public static event System.Action OnBuildingChanged;

    private readonly HashSet<int> _occupiedSlots = new();
    public int OccupiedCount => _occupiedSlots.Count;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OccupySlot(int slotId) { _occupiedSlots.Add(slotId); OnBuildingChanged?.Invoke(); }
    public void FreeSlot(int slotId)   { _occupiedSlots.Remove(slotId); OnBuildingChanged?.Invoke(); }
    public bool IsSlotOccupied(int slotId) => _occupiedSlots.Contains(slotId);
}
