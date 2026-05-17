using UnityEngine;
using System.Collections;

public class TroopSpawner : MonoBehaviour
{
    [SerializeField] GameObject troopPrefab;
    [SerializeField] float spawnCooldownLv1 = 8f;
    [SerializeField] float spawnCooldownLv2 = 4f;

    Building _building;
    float _cooldown;
    bool _running;

    void Awake()
    {
        _building = GetComponent<Building>();
        _cooldown = spawnCooldownLv1;
    }

    void Start()
    {
        if (_building != null) _building.OnDestroyed += StopSpawning;
        StartSpawning();
    }

    public void OnUpgrade() => _cooldown = spawnCooldownLv2;

    void StartSpawning() { _running = true; StartCoroutine(SpawnLoop()); }
    void StopSpawning()  { _running = false; StopAllCoroutines(); }

    IEnumerator SpawnLoop()
    {
        while (_running)
        {
            yield return new WaitForSeconds(_cooldown);
            while (ResourceManager.Instance != null && ResourceManager.Instance.IsTroopCapReached())
                yield return new WaitForSeconds(1f);
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (troopPrefab == null) return;
        if (ResourceManager.Instance == null) return;

        var go = Instantiate(troopPrefab, transform.position, Quaternion.identity);
        if (go.TryGetComponent<Unit>(out var unit))
            unit.Initialize(_building != null ? _building.Faction : Faction.Blue);
        ResourceManager.Instance.RegisterTroop();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.unitSpawn);
    }
}
