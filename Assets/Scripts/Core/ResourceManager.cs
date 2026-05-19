using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public static event System.Action OnResourceChanged;
    public static event System.Action<int> OnTroopCapChanged;

    [Header("Starting Resources")]
    [SerializeField] int startingWood = 300;   // more wood to build multiple structures
    [SerializeField] int startingGold = 200;   // more gold so troops spawn without stalling
    [SerializeField] int startingFood = 50;

    private Dictionary<ResourceType, int> _resources;

    private int _pawnSlotCap = 3;   // base 3; +1 per House built
    public int PawnSlotCap => _pawnSlotCap;

    public int ActivePawnCount { get; private set; }
    public int ActiveTroopCount { get; private set; }
    public int TroopCap => CalculateTroopCap(_resources[ResourceType.Food]);

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _resources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, startingWood },
            { ResourceType.Gold, startingGold },
            { ResourceType.Food, startingFood }
        };
    }

    public static int CalculateTroopCap(int food) =>
        Mathf.Clamp(5 + food / 10, 5, 20);

    public int GetAmount(ResourceType type) => _resources[type];

    public bool CanSpend(ResourceType type, int amount) =>
        _resources[type] >= amount;

    public void AddResource(ResourceType type, int amount)
    {
        _resources[type] += amount;
        OnResourceChanged?.Invoke();
        if (type == ResourceType.Food) OnTroopCapChanged?.Invoke(TroopCap);
    }

    public bool Spend(ResourceType type, int amount)
    {
        if (!CanSpend(type, amount)) return false;
        _resources[type] -= amount;
        OnResourceChanged?.Invoke();
        return true;
    }

    public void RegisterTroop() => ActiveTroopCount++;

    public void UnregisterTroop() =>
        ActiveTroopCount = Mathf.Max(0, ActiveTroopCount - 1);

    public bool IsTroopCapReached() => ActiveTroopCount >= TroopCap;

    public void RegisterPawn()   => ActivePawnCount++;
    public void UnregisterPawn() => ActivePawnCount = Mathf.Max(0, ActivePawnCount - 1);

    public void IncrementPawnSlotCap() => _pawnSlotCap++;
}
