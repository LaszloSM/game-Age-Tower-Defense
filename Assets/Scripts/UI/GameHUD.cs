using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameHUD : UIManager
{
    [Header("Castle HP")]
    [SerializeField] Image castleHPFill;

    [Header("Resources")]
    [SerializeField] TextMeshProUGUI woodText;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI foodText;

    [Header("Timer")]
    [SerializeField] TextMeshProUGUI timerText;

    [Header("Build Buttons")]
    [SerializeField] Button barracksBtn;
    [SerializeField] Button archeryBtn;
    [SerializeField] Button towerBtn;
    [SerializeField] Button monasteryBtn;
    [SerializeField] Button houseBtn;

    [Header("Special Buttons")]
    [SerializeField] Button troopBoostBtn;
    [SerializeField] Button castleRepairBtn;
    [SerializeField] Button buyPawnBtn;
    [SerializeField] Button upgradeBtn;

    [Header("Wave Banner (optional)")]
    [Tooltip("A panel that fades in/out when waves start or are cleared.")]
    [SerializeField] GameObject waveBannerRoot;
    [SerializeField] TMP_Text   waveBannerText;
    [SerializeField] TMP_Text   waveCountdownText;

    Castle _castle;
    float _elapsed;
    float _boostCooldown;
    Building _selectedBuilding;
    BuildingSlot _pendingSlot;

    protected override void Awake()
    {
        base.Awake();
        _castle = FindFirstObjectByType<Castle>();
    }

    void Start()
    {
        if (_castle != null) _castle.OnHealthChanged += UpdateCastleHP;

        BuildingSlot.OnSlotClicked += OnSlotClicked;
        Building.OnBuildingClicked += OnBuildingSelected;

        barracksBtn.onClick.AddListener(() => PlaceBuilding(BuildingType.Barracks));
        archeryBtn.onClick.AddListener(()  => PlaceBuilding(BuildingType.Archery));
        towerBtn.onClick.AddListener(()    => PlaceBuilding(BuildingType.Tower));
        monasteryBtn.onClick.AddListener(() => PlaceBuilding(BuildingType.Monastery));
        houseBtn.onClick.AddListener(()    => PlaceBuilding(BuildingType.House));

        troopBoostBtn.onClick.AddListener(OnTroopBoost);
        castleRepairBtn.onClick.AddListener(() => _castle?.EmergencyRepair());
        buyPawnBtn.onClick.AddListener(OnBuyPawn);
        upgradeBtn.onClick.AddListener(OnUpgradeBuilding);

        AudioManager.Instance?.PlayMusic(AudioManager.Instance.gameMusic);
        RefreshHUD();
    }

    void OnEnable()
    {
        WaveManager.OnWaveStarting  += HandleWaveStarting;
        WaveManager.OnWaveBegun     += HandleWaveBegun;
        WaveManager.OnWaveCleared   += HandleWaveCleared;
        WaveManager.OnWaveCountdown += HandleWaveCountdown;
    }

    void OnDisable()
    {
        WaveManager.OnWaveStarting  -= HandleWaveStarting;
        WaveManager.OnWaveBegun     -= HandleWaveBegun;
        WaveManager.OnWaveCleared   -= HandleWaveCleared;
        WaveManager.OnWaveCountdown -= HandleWaveCountdown;
    }

    void OnDestroy()
    {
        if (_castle != null) _castle.OnHealthChanged -= UpdateCastleHP;
        BuildingSlot.OnSlotClicked -= OnSlotClicked;
        Building.OnBuildingClicked -= OnBuildingSelected;
    }

    void Update()
    {
        if (GameManager.Instance?.State == GameState.Playing)
        {
            _elapsed += Time.deltaTime;
            int m = (int)(_elapsed / 60f);
            int s = (int)(_elapsed % 60f);
            timerText.text = $"{m:00}:{s:00}";
        }

        if (_boostCooldown > 0f)
        {
            _boostCooldown -= Time.deltaTime;
            troopBoostBtn.interactable = _boostCooldown <= 0f;
        }
    }

    protected override void RefreshHUD()
    {
        if (ResourceManager.Instance == null) return;
        woodText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Wood)}";
        goldText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Gold)}";
        int food = ResourceManager.Instance.GetAmount(ResourceType.Food);
        int cap  = ResourceManager.Instance.TroopCap;
        foodText.text = $"{food} / cap:{cap}";
        RefreshBuildButtons();
    }

    void UpdateCastleHP(float current, float max)
    {
        if (castleHPFill != null) castleHPFill.fillAmount = current / max;
    }

    void RefreshBuildButtons()
    {
        var rm = ResourceManager.Instance;
        if (rm == null) return;
        barracksBtn.interactable  = rm.CanSpend(ResourceType.Wood, 80);
        archeryBtn.interactable   = rm.CanSpend(ResourceType.Wood, 60);
        towerBtn.interactable     = rm.CanSpend(ResourceType.Wood, 100);
        monasteryBtn.interactable = rm.CanSpend(ResourceType.Wood, 70) && rm.CanSpend(ResourceType.Food, 30);
        houseBtn.interactable     = rm.CanSpend(ResourceType.Wood, 50);
        buyPawnBtn.interactable   = rm.CanSpend(ResourceType.Gold, 50)
                                  && rm.ActivePawnCount < rm.PawnSlotCap;
        upgradeBtn.interactable   = _selectedBuilding != null
                                  && _selectedBuilding.Level < 2
                                  && rm.CanSpend(ResourceType.Gold, 100);
    }

    void OnSlotClicked(BuildingSlot slot)
    {
        _pendingSlot = slot;
        ShowBuildPanel();
    }

    void PlaceBuilding(BuildingType type)
    {
        if (_pendingSlot == null) return;
        BuildingPlacer.Place(type, _pendingSlot);
        _pendingSlot = null;
        HideBuildPanel();
    }

    void OnBuildingSelected(Building building)
    {
        _selectedBuilding = building;
        ShowBuildingInfo();
        RefreshBuildButtons();
    }

    void OnUpgradeBuilding()
    {
        if (_selectedBuilding == null || _selectedBuilding.Level >= 2) return;
        if (!ResourceManager.Instance.Spend(ResourceType.Gold, 100)) return;
        _selectedBuilding.Upgrade();
        RefreshBuildButtons();
    }

    void OnBuyPawn()
    {
        var rm = ResourceManager.Instance;
        if (rm == null) return;
        if (rm.ActivePawnCount >= rm.PawnSlotCap) return;
        if (!rm.Spend(ResourceType.Gold, 50)) return;
        FindFirstObjectByType<PawnSpawner>()?.SpawnOne();
        RefreshBuildButtons();
    }

    void OnTroopBoost()
    {
        if (_boostCooldown > 0f) return;
        if (!ResourceManager.Instance.Spend(ResourceType.Gold, 150)) return;
        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var u in units)
            if (u.UnitFaction == FactionSelectUI.ChosenFaction)
                u.ApplyStatMultiplier(1.5f);
        _boostCooldown = 120f;
        troopBoostBtn.interactable = false;
    }

    public float GetElapsedTime() => _elapsed;

    // ─── Wave Banner ──────────────────────────────────────────────────────────

    void HandleWaveStarting(int wave)
    {
        ShowBanner($"Oleada {wave} en...");
        if (waveCountdownText != null) waveCountdownText.gameObject.SetActive(true);
    }

    void HandleWaveCountdown(int secondsLeft)
    {
        if (waveCountdownText != null) waveCountdownText.text = secondsLeft.ToString();
    }

    void HandleWaveBegun(int wave)
    {
        if (waveCountdownText != null) waveCountdownText.gameObject.SetActive(false);
        ShowBanner($"¡Oleada {wave}!", 3f);
    }

    void HandleWaveCleared(int wave)
    {
        ShowBanner($"¡Oleada {wave} superada!", 4f);
    }

    void ShowBanner(string message, float hideAfter = 0f)
    {
        if (waveBannerRoot == null) return;
        if (waveBannerText != null) waveBannerText.text = message;
        waveBannerRoot.SetActive(true);
        if (hideAfter > 0f)
        {
            StopCoroutine(nameof(HideBannerAfter));
            StartCoroutine(HideBannerAfter(hideAfter));
        }
    }

    IEnumerator HideBannerAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        waveBannerRoot?.SetActive(false);
    }

#if UNITY_EDITOR
    void LateUpdate()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.D) && _castle != null)
            _castle.TakeDamage(100f);
    }
#endif
}
