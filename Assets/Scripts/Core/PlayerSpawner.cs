using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject blueWarriorPrefab;
    [SerializeField] GameObject redWarriorPrefab;
    [SerializeField] Transform  spawnPoint;

    void Start()
    {
        var prefab = FactionSelectUI.ChosenFaction == Faction.Blue
            ? blueWarriorPrefab
            : redWarriorPrefab;

        if (prefab == null) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        var go = Instantiate(prefab, pos, Quaternion.identity);

        // Swap AI for player control immediately (DestroyImmediate prevents AI Update from running)
        var ai = go.GetComponent<WarriorAI>();
        if (ai != null) DestroyImmediate(ai);

        go.AddComponent<PlayerWarrior>();
    }
}
