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

    [Header("Close Button")]
    [SerializeField] Button closeBuildPanelBtn;

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

        if (barracksBtn != null) barracksBtn.onClick.AddListener(() => PlaceBuilding(BuildingType.Barracks));
        if (archeryBtn != null) archeryBtn.onClick.AddListener(()  => PlaceBuilding(BuildingType.Archery));
        if (towerBtn != null) towerBtn.onClick.AddListener(()    => PlaceBuilding(BuildingType.Tower));
        if (monasteryBtn != null) monasteryBtn.onClick.AddListener(() => PlaceBuilding(BuildingType.Monastery));
        if (houseBtn != null) houseBtn.onClick.AddListener(()    => PlaceBuilding(BuildingType.House));

        if (troopBoostBtn != null) troopBoostBtn.onClick.AddListener(OnTroopBoost);
        if (castleRepairBtn != null) castleRepairBtn.onClick.AddListener(() => _castle?.EmergencyRepair());
        if (buyPawnBtn != null) buyPawnBtn.onClick.AddListener(OnBuyPawn);
        if (upgradeBtn != null) upgradeBtn.onClick.AddListener(OnUpgradeBuilding);
        if (closeBuildPanelBtn != null) closeBuildPanelBtn.onClick.AddListener(HideBuildPanel);

        AudioManager.Instance?.PlayMusic(AudioManager.Instance.gameMusic);
        RefreshHUD();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        WaveManager.OnWaveStarting  += HandleWaveStarting;
        WaveManager.OnWaveBegun     += HandleWaveBegun;
        WaveManager.OnWaveCleared   += HandleWaveCleared;
        WaveManager.OnWaveCountdown += HandleWaveCountdown;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
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
            if (timerText != null) timerText.text = $"{m:00}:{s:00}";
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
        if (woodText != null) woodText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Wood)}";
        if (goldText != null) goldText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Gold)}";
        int food = ResourceManager.Instance.GetAmount(ResourceType.Food);
        int cap  = ResourceManager.Instance.TroopCap;
        if (foodText != null) foodText.text = $"{food}/{cap}";
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

        if (barracksBtn != null) barracksBtn.interactable  = rm.CanSpend(ResourceType.Wood, 80);
        if (archeryBtn != null) archeryBtn.interactable   = rm.CanSpend(ResourceType.Wood, 60);
        if (towerBtn != null) towerBtn.interactable     = rm.CanSpend(ResourceType.Wood, 100);
        if (monasteryBtn != null) monasteryBtn.interactable = rm.CanSpend(ResourceType.Wood, 70) && rm.CanSpend(ResourceType.Food, 30);
        if (houseBtn != null) houseBtn.interactable     = rm.CanSpend(ResourceType.Wood, 50);
        
        if (buyPawnBtn != null) 
            buyPawnBtn.interactable = rm.CanSpend(ResourceType.Gold, 50) && rm.ActivePawnCount < rm.PawnSlotCap;

        if (upgradeBtn != null) 
            upgradeBtn.interactable = _selectedBuilding != null && _selectedBuilding.Level < 2 && rm.CanSpend(ResourceType.Gold, 100);
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

    // â”€â”€â”€ Wave Banner â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
        ShowBanner($"OLEADA {wave}", 3f);
    }

    void HandleWaveCleared(int wave)
    {
        ShowBanner($"Oleada {wave} superada", 4f);
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


}
