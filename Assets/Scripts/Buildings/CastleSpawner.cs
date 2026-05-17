using UnityEngine;

public class CastleSpawner : MonoBehaviour
{
    [SerializeField] GameObject blueCastlePrefab;
    [SerializeField] GameObject redCastlePrefab;
    [SerializeField] Transform castleSpawnPoint;

    void Awake()    // Awake so Castle exists before WaveManager.Start() runs
    {
        GameObject prefab = FactionSelectUI.ChosenFaction == Faction.Blue
            ? blueCastlePrefab : redCastlePrefab;
        if (prefab != null && castleSpawnPoint != null)
            Instantiate(prefab, castleSpawnPoint.position, Quaternion.identity);
    }
}
