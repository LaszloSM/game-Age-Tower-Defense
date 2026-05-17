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

    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyWarriorPrefab;
    [SerializeField] GameObject enemyArcherPrefab;
    [SerializeField] GameObject enemyLancerPrefab;
    [SerializeField] GameObject enemyMonkPrefab;

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
        // Start waves if the game is Playing — or if there's no GameManager at all
        // (allows direct Game-scene testing without the full menu flow).
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
            float countdown = restBetweenWaves;
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
        int count = EnemiesForWave(wave);
        for (int i = 0; i < count; i++)
        {
            SpawnOne(wave);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ─── Spawning ─────────────────────────────────────────────────────────────

    void SpawnOne(int wave)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        GameObject prefab = ChoosePrefab(wave);
        if (prefab == null) return;

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var go = Instantiate(prefab, spawn.position, Quaternion.identity);

        if (go.TryGetComponent<Unit>(out var unit))
        {
            unit.Initialize(_enemyFaction);
            ApplyScaling(unit, wave);
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

    GameObject ChoosePrefab(int wave)
    {
        if (wave == 1) return enemyWarriorPrefab;
        if (wave == 2) return Random.value > 0.4f ? enemyWarriorPrefab : enemyArcherPrefab;

        float r = Random.value;
        if (wave <= 4)
        {
            if (r < 0.45f) return enemyWarriorPrefab;
            if (r < 0.75f) return enemyArcherPrefab;
            return enemyLancerPrefab;
        }

        // Wave 5+: full mix
        if (r < 0.30f) return enemyWarriorPrefab;
        if (r < 0.55f) return enemyArcherPrefab;
        if (r < 0.75f) return enemyLancerPrefab;
        return enemyMonkPrefab;
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
