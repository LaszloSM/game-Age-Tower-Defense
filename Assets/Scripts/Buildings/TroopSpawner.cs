using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TroopSpawner : MonoBehaviour
{
    [SerializeField] GameObject troopPrefab;
    [Tooltip("Alternate prefab used by ENEMY buildings when player chose Red faction (so enemies look Blue). Leave empty on allied buildings.")]
    [SerializeField] GameObject troopPrefabAlt;
    [SerializeField] float spawnCooldownLv1 = 3f;   // seconds between spawns (level 1)
    [SerializeField] float spawnCooldownLv2 = 2f;   // seconds between spawns (level 2)

    [Header("Production Limits")]
    [Tooltip("Maximum troops alive at once from this single building.")]
    [SerializeField] int   maxTroopsAlive   = 3;
    [Tooltip("Gold deducted per allied troop. Enemy buildings ignore this.")]
    [SerializeField] int   goldCostPerTroop = 10;   // reduced from 15 so economy flows better
    [Tooltip("HP override for allied troops at spawn (0 = use prefab default).")]
    [SerializeField] float allySpawnHP      = 200f;  // allies are tankier than default

    // ── Runtime state ───────────────────────────────────────────────────────
    Building      _building;
    float         _cooldown;
    bool          _running;
    int           _activeTroops;
    bool          _isAlly;
    List<Unit>    _spawnedUnits = new List<Unit>();

    void Awake()
    {
        _building = GetComponent<Building>();
        _cooldown  = spawnCooldownLv1;
    }

    void Start()
    {
        _isAlly = _building != null
            ? _building.Faction == FactionSelectUI.ChosenFaction
            : true;

        if (_building != null) _building.OnDestroyed += OnBuildingDestroyed;
        StartSpawning();
    }

    public void OnUpgrade() => _cooldown = spawnCooldownLv2;

    void OnBuildingDestroyed()
    {
        StopSpawning();

        if (_isAlly)
        {
            // Allied troops survive and regroup near the player's castle
            Castle[] castles = Object.FindObjectsByType<Castle>(FindObjectsSortMode.None);
            Castle allied = null;
            foreach (var c in castles)
                if (c.Faction == FactionSelectUI.ChosenFaction) { allied = c; break; }

            if (allied != null)
            {
                Vector3 castlePos = allied.transform.position;
                foreach (var u in _spawnedUnits)
                    if (u != null && !u.IsDead)
                        u.SetDefendPosition(castlePos, 8f);
            }
        }
        else
        {
            // Enemy troops freed from their building aggressively hunt the player's castle
            Castle[] castles = Object.FindObjectsByType<Castle>(FindObjectsSortMode.None);
            Castle playerCastle = null;
            foreach (var c in castles)
                if (c.Faction == FactionSelectUI.ChosenFaction) { playerCastle = c; break; }

            if (playerCastle != null)
            {
                Vector3 target = playerCastle.transform.position;
                foreach (var u in _spawnedUnits)
                    if (u != null && !u.IsDead)
                        u.SetDefendPosition(target, 20f);  // large radius = hunt everything
            }
        }

        _spawnedUnits.Clear();
    }

    void StartSpawning() { _running = true; StartCoroutine(SpawnLoop()); }
    void StopSpawning()  { _running = false; StopAllCoroutines(); }

    IEnumerator SpawnLoop()
    {
        // Very short warm-up so the scene is fully loaded
        yield return new WaitForSeconds(0.5f);

        while (_running)
        {
            // Wait for cooldown
            yield return new WaitForSeconds(_cooldown);

            if (_isAlly)
            {
                // Allied: per-building cap + global cap + gold check
                while (_activeTroops >= maxTroopsAlive
                       || (ResourceManager.Instance != null && ResourceManager.Instance.IsTroopCapReached())
                       || (ResourceManager.Instance != null && !ResourceManager.Instance.CanSpend(ResourceType.Gold, goldCostPerTroop)))
                    yield return new WaitForSeconds(0.5f);
            }
            else
            {
                // Enemy buildings: only per-building cap
                while (_activeTroops >= maxTroopsAlive)
                    yield return new WaitForSeconds(0.5f);
            }

            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (troopPrefab == null)
        {
            Debug.LogWarning($"[TroopSpawner] {gameObject.name}: troopPrefab not assigned!", this);
            return;
        }

        if (_isAlly)
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;
            if (!rm.Spend(ResourceType.Gold, goldCostPerTroop)) return;
        }

        Faction faction  = _building != null ? _building.Faction : FactionSelectUI.ChosenFaction;
        Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 1.5f);

        // Enemy buildings: when player is Red the enemy is Blue — use troopPrefabAlt (Blue sprite) if assigned.
        bool useAlt = !_isAlly
                   && FactionSelectUI.ChosenFaction == Faction.Red
                   && troopPrefabAlt != null;
        GameObject selectedPrefab = useAlt ? troopPrefabAlt : troopPrefab;

        var go = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        if (!go.TryGetComponent<Unit>(out var unit))
        {
            Debug.LogWarning($"[TroopSpawner] Prefab '{troopPrefab.name}' has no Unit!", this);
            return;
        }

        // Allied troops guard a wide zone around the player's castle.
        // Enemy building troops stay near their own building.
        Vector3 patrolCenter = transform.position;
        float   patrolRadius = 10f;
        if (_isAlly)
        {
            Castle[] castles = Object.FindObjectsByType<Castle>(FindObjectsSortMode.None);
            foreach (var c in castles)
            {
                if (c.Faction == FactionSelectUI.ChosenFaction)
                {
                    patrolCenter = c.transform.position;
                    patrolRadius = 15f;   // wide zone so troops spread visibly
                    break;
                }
            }
        }

        unit.InitializeDefender(faction, patrolCenter, patrolRadius);
        unit.MoveSpeed = _isAlly ? 9f : 6f;

        if (_isAlly)
        {
            // Boost HP so allied troops survive long enough to be useful
            if (allySpawnHP > 0f) unit.SetMaxHP(allySpawnHP);
        }
        else
        {
            unit.AttackDamage = 3f;   // enemy building troops deal same low damage as wave enemies
        }

        unit.OnDied += OnTroopDied;
        _spawnedUnits.Add(unit);
        _activeTroops++;

        if (_isAlly)
            ResourceManager.Instance?.RegisterTroop();

        // Spawn particle to make troops visible when they appear
        ParticleSpawner.Spawn(ParticleSpawner.Dust, spawnPos);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.unitSpawn);
    }

    void OnTroopDied()
    {
        _activeTroops = Mathf.Max(0, _activeTroops - 1);
        _spawnedUnits.RemoveAll(u => u == null || u.IsDead);
    }
}
