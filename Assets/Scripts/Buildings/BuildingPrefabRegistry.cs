using System.Collections.Generic;
using UnityEngine;

public class BuildingPrefabRegistry : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        public BuildingType type;
        public GameObject bluePrefab;
        public GameObject redPrefab;
    }

    [SerializeField] Entry[] entries;

    void Awake()
    {
        BuildingPlacer.FactionPrefabs = new Dictionary<BuildingType, GameObject[]>();
        foreach (var e in entries)
            BuildingPlacer.FactionPrefabs[e.type] = new[] { e.bluePrefab, e.redPrefab };
    }
}
