using UnityEngine;

public class PawnSpawner : MonoBehaviour
{
    [SerializeField] GameObject bluePawnPrefab;
    [SerializeField] GameObject redPawnPrefab;
    [SerializeField] Transform[] startPositions;

    GameObject _prefab;

    void Start()
    {
        _prefab = FactionSelectUI.ChosenFaction == Faction.Blue
            ? bluePawnPrefab : redPawnPrefab;
        foreach (var pos in startPositions)
            Instantiate(_prefab, pos.position, Quaternion.identity);
    }

    public void SpawnOne()
    {
        if (_prefab == null || startPositions == null || startPositions.Length == 0) return;
        Instantiate(_prefab, startPositions[0].position, Quaternion.identity);
    }
}
