using System.Collections.Generic;
using UnityEngine;

public static class BuildingPlacer
{
    // [BluePrefab, RedPrefab] per type — wired by BuildingPrefabRegistry on scene start
    public static Dictionary<BuildingType, GameObject[]> FactionPrefabs = new();

    public static void Place(BuildingType type, BuildingSlot slot)
    {
        if (slot == null || slot.IsOccupied) return;
        var rm = ResourceManager.Instance;
        if (rm == null) return;
        if (!DeductCost(type, rm)) return;

        Faction faction = FactionSelectUI.ChosenFaction;
        if (!FactionPrefabs.TryGetValue(type, out var prefabs)) return;
        GameObject prefab = faction == Faction.Blue ? prefabs[0] : prefabs[1];
        if (prefab == null) return;

        var go = Object.Instantiate(prefab, slot.transform.position, Quaternion.identity);
        slot.Occupy(go);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buildingPlace);
    }

    static bool DeductCost(BuildingType type, ResourceManager rm)
    {
        return type switch
        {
            BuildingType.Barracks  => rm.Spend(ResourceType.Wood, 80),
            BuildingType.Archery   => rm.Spend(ResourceType.Wood, 60),
            BuildingType.Tower     => rm.Spend(ResourceType.Wood, 100),
            // Food is a gate (stored total), NOT consumed — spec Section 4.3
            BuildingType.Monastery => rm.CanSpend(ResourceType.Food, 30)
                                   && rm.Spend(ResourceType.Wood, 70),
            BuildingType.House     => rm.Spend(ResourceType.Wood, 50),
            _ => false
        };
    }
}
