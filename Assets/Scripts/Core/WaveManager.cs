using System.Collections;
using UnityEngine;

/// <summary>
/// Sends enemy waves from multiple map-edge spawn points toward the castle.
/// Wave N starts after a 30-second "rest" period following the previous wave.
/// Raises events so the HUD can display "Wave 3 incoming!" banners.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Enemy Prefabs — Red team (player = Blue)")]
    [SerializeField] GameObject enemyWarriorPrefab;
    [SerializeField] GameObject enemyArcherPrefab;
    [SerializeField] GameObject enemyLancerPrefab;
    [SerializeField] GameObject enemyMonkPrefab;

    [Header("Enemy Prefabs — Blue team (player = Red)")]
    [Tooltip("Blue-sprite warrior spawned as enemy when player chose Red faction.")]
    [SerializeField] GameObject blueWarriorPrefab;
    [Tooltip("Blue-sprite archer spawned as enemy when player chose Red faction.")]
    [SerializeField] GameObject blueArcherPrefab;
    [Tooltip("Blue-sprite lancer spawned as enemy when player chose Red faction.")]
    [SerializeField] GameObject blueLancerPrefab;
    [Tooltip("Blue-sprite monk spawned as enemy when player chose Red faction.")]
    [SerializeField] GameObject blueMonkPrefab;

    [Header("Spawn Points (map edges)")]
    [Tooltip("Add one Transform per map-edge spawn location. Enemies pick a random one each spawn.")]
    [SerializeField] Transform[] spawnPoints;

    [Header("Wave Timing")]
    [SerializeField] float restBetweenWaves = 30f;   // calm period before each new wave
    [SerializeField] float spawnInterval    =  1.5f; // seconds between individual spawns within a wave

    // ─── Events (subscribe from GameHUD to show wave banners) ────────────────
    public static event System.Action<int>  OnWaveStarting;    // fired restBetweenWaves BEFORE wave starts
    public static event System.Action<int>  OnWaveBegun;       // fired when first enemy of wave spawns
    public static event System.Action<int>  OnWaveCleared;     // fired when last enemy of a wave dies
    public static event System.Action<int>  OnWaveCountdown;   // fired every second during countdown (int = seconds left)

    Faction _enemyFaction;
    bool    _running;
    int     _waveNumber;   // 1-based
    int     _activeEnemies;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _enemyFaction = FactionSelectUI.ChosenFaction == Faction.Blue ? Faction.Red : Faction.Blue;

        // If GameManager exists but the game was never transitioned to Playing
        // (e.g. direct Game-scene play in the editor), force the transition now.
        // ChangeState fires OnStateChanged → HandleState(Playing) → StartCoroutine(WaveLoop())
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Menu)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
            return; // HandleState already started the coroutine
        }

        // Normal path: either no GameManager (unit-test scene) or already Playing
        bool shouldStart = GameManager.Instance == null
                        || GameManager.Instance.State == GameState.Playing;
        if (shouldStart)
            StartCoroutine(WaveLoop());
    }

    void OnEnable()  => GameManager.OnStateChanged += HandleState;
    void OnDisable() => GameManager.OnStateChanged -= HandleState;

    void HandleState(GameState state)
    {
        if (state == GameState.Playing)
        {
            if (!_running) StartCoroutine(WaveLoop());
        }
        else
        {
            _running = false;
            StopAllCoroutines();
        }
    }

    // ─── Main Loop ────────────────────────────────────────────────────────────

    IEnumerator WaveLoop()
    {
        _running = true;

        while (_running)
        {
            _waveNumber++;

            // ── Countdown ────────────────────────────────────────────────────
            OnWaveStarting?.Invoke(_waveNumber);
            float countdown = _waveNumber == 1 ? 30f : 10f; // 30s for first wave, 10s for others
            while (countdown > 0f)
            {
                OnWaveCountdown?.Invoke(Mathf.CeilToInt(countdown));
                yield return new WaitForSeconds(1f);
                countdown -= 1f;
            }

            // ── Spawn wave ───────────────────────────────────────────────────
            OnWaveBegun?.Invoke(_waveNumber);
            yield return StartCoroutine(SpawnWave(_waveNumber));

            // ── Wait until all wave enemies are dead ─────────────────────────
            yield return new WaitUntil(() => _activeEnemies <= 0);
            OnWaveCleared?.Invoke(_waveNumber);
        }
    }

    IEnumerator SpawnWave(int wave)
    {
        int totalToSpawn = Mathf.Min(EnemiesForWave(wave), 25);
        int spawnedCount = 0;
        int lastSpawnIdx = -1;   // track last group's spawn point so next one differs

        while (spawnedCount < totalToSpawn)
        {
            // Groups of 3–5, each group from a DIFFERENT spawn point
            int groupSize   = Random.Range(3, 6);
            int actualGroup = Mathf.Min(groupSize, totalToSpawn - spawnedCount);

            // Pick a spawn point different from the last one (forces flanking)
            int spawnIdx = PickDifferentSpawn(lastSpawnIdx);
            lastSpawnIdx  = spawnIdx;

            for (int i = 0; i < actualGroup; i++)
            {
                SpawnOneAt(wave, spawnIdx);
                spawnedCount++;
                if (i < actualGroup - 1)
                    yield return new WaitForSeconds(0.7f);  // small gap inside group
            }

            if (spawnedCount < totalToSpawn)
                yield return new WaitForSeconds(6f);  // gap between groups keeps battlefield readable
        }
    }

    int PickDifferentSpawn(int lastIdx)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return 0;
        if (spawnPoints.Length == 1) return 0;

        int idx;
        int tries = 0;
        do {
            idx = Random.Range(0, spawnPoints.Length);
            tries++;
        } while (idx == lastIdx && tries < 10);
        return idx;
    }

    // ─── Spawning ─────────────────────────────────────────────────────────────

    void SpawnOneAt(int wave, int spawnIdx)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        GameObject prefab = ChoosePrefab(wave);
        if (prefab == null) return;

        Transform spawn  = spawnPoints[spawnIdx];
        // Wider scatter so enemies don't spawn on top of each other
        Vector3   offset = (Vector3)(Random.insideUnitCircle * 2.5f);
        var go = Instantiate(prefab, spawn.position + offset, Quaternion.identity);

        if (go.TryGetComponent<Unit>(out var unit))
        {
            unit.Initialize(_enemyFaction);
            unit.MoveSpeed    = 6f;
            unit.AttackDamage = 3f;
            ApplyScaling(unit, wave);
            // Large per-unit path offset keeps troops in separate lanes — no clustering at waypoints
            float lateralRange = 4.5f;   // ±4.5 Y  → wide spread across the path
            float xJitter      = 1.8f;   // ±1.8 X  → stagger arrival times at each node
            unit.SetPathOffset(new Vector3(
                Random.Range(-xJitter,      xJitter),
                Random.Range(-lateralRange, lateralRange), 0f));
            unit.OnDied += OnEnemyDied;
            _activeEnemies++;
        }
    }

    void OnEnemyDied()
    {
        _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
    }

    // ─── Wave Composition ─────────────────────────────────────────────────────

    int EnemiesForWave(int wave) => wave switch
    {
        1 => 5,
        2 => 8,
        3 => 10,
        4 => 12,
        _ => 10 + wave * 2   // scales indefinitely
    };

    // Returns the correct prefab set based on which faction is the enemy this session.
    // When player chose Blue  → enemy is Red  → use enemyXxxPrefab (Red sprites).
    // When player chose Red   → enemy is Blue → use blueXxxPrefab  (Blue sprites).
    GameObject WarriorPrefab => _enemyFaction == Faction.Blue && blueWarriorPrefab != null ? blueWarriorPrefab : enemyWarriorPrefab;
    GameObject ArcherPrefab  => _enemyFaction == Faction.Blue && blueArcherPrefab  != null ? blueArcherPrefab  : enemyArcherPrefab;
    GameObject LancerPrefab  => _enemyFaction == Faction.Blue && blueLancerPrefab  != null ? blueLancerPrefab  : enemyLancerPrefab;
    GameObject MonkPrefab    => _enemyFaction == Faction.Blue && blueMonkPrefab    != null ? blueMonkPrefab    : enemyMonkPrefab;

    GameObject ChoosePrefab(int wave)
    {
        if (wave == 1) return WarriorPrefab;
        if (wave == 2) return Random.value > 0.4f ? WarriorPrefab : ArcherPrefab;

        float r = Random.value;

        // Waves 3-6: warriors + archers + lancers only
        if (wave <= 6)
        {
            if (r < 0.45f) return WarriorPrefab;
            if (r < 0.75f) return ArcherPrefab;
            return LancerPrefab;
        }

        // Wave 7+: full mix including monks
        if (r < 0.30f) return WarriorPrefab;
        if (r < 0.55f) return ArcherPrefab;
        if (r < 0.75f) return LancerPrefab;
        return MonkPrefab;
    }

    void ApplyScaling(Unit unit, int wave)
    {
        if (wave <= 3) return;
        float mult = 1f + 0.25f * (wave - 3);   // +25 % per wave after wave 3
        unit.ApplyStatMultiplier(mult);
    }

    // ─── Public Query ─────────────────────────────────────────────────────────
    public int CurrentWave    => _waveNumber;
    public int ActiveEnemies  => _activeEnemies;
}
