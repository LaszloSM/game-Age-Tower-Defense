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
        pausePanel?.SetActive(state == GameState.Paused);
        gameOverPanel?.SetActive(state == GameState.GameOver);
    }

    public void ShowBuildPanel()    => buildPanel?.SetActive(true);
    public void HideBuildPanel()    => buildPanel?.SetActive(false);
    public void ShowPawnInfo()      => pawnInfoPanel?.SetActive(true);
    public void HidePawnInfo()      => pawnInfoPanel?.SetActive(false);
    public void ShowBuildingInfo()  => buildingInfoPanel?.SetActive(true);
    public void HideBuildingInfo()  => buildingInfoPanel?.SetActive(false);

    protected virtual void RefreshHUD() { }
}
