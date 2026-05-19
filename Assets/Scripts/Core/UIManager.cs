using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels (assign in Inspector)")]
    [SerializeField] protected GameObject pausePanel;
    [SerializeField] protected GameObject gameOverPanel;
    [SerializeField] protected GameObject buildPanel;
    [SerializeField] protected GameObject pawnInfoPanel;
    [SerializeField] protected GameObject buildingInfoPanel;

    protected virtual void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    protected virtual void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
        ResourceManager.OnResourceChanged += RefreshHUD;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
        ResourceManager.OnResourceChanged -= RefreshHUD;
    }

    void HandleStateChanged(GameState state)
    {
        if (pausePanel != null) pausePanel.SetActive(state == GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);
    }

    public void ShowBuildPanel()    { if(buildPanel != null) buildPanel.SetActive(true); }
    public void HideBuildPanel()    { if(buildPanel != null) buildPanel.SetActive(false); }
    public void ShowPawnInfo()      { if(pawnInfoPanel != null) pawnInfoPanel.SetActive(true); }
    public void HidePawnInfo()      { if(pawnInfoPanel != null) pawnInfoPanel.SetActive(false); }
    public void ShowBuildingInfo()  { if(buildingInfoPanel != null) buildingInfoPanel.SetActive(true); }
    public void HideBuildingInfo()  { if(buildingInfoPanel != null) buildingInfoPanel.SetActive(false); }

    protected virtual void RefreshHUD() { }
}
