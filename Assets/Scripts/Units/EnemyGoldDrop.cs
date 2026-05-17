using UnityEngine;

// Attach ONLY to enemy unit prefabs (not player units)
public class EnemyGoldDrop : MonoBehaviour
{
    [SerializeField] int goldDrop = 5;  // set per-prefab: Warrior=3, Archer=5, Lancer=7, Monk=10

    void Start()
    {
        if (TryGetComponent<Unit>(out var unit))
            unit.OnDied += () => ResourceManager.Instance?.AddResource(ResourceType.Gold, goldDrop);
    }
}
