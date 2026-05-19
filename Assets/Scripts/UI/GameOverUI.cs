using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI survivalTimeText;
    [SerializeField] TextMeshProUGUI goldEarnedText;
    [SerializeField] Button retryButton;
    [SerializeField] Button menuButton;

    void Start()
    {
        retryButton.onClick.AddListener(() => GameManager.Instance?.StartGame());
        menuButton.onClick.AddListener(() => GoToMenu());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) GoToMenu();
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;   // reset in case game was paused
        GameManager.Instance?.GoToMainMenu();
    }

    void OnEnable()
    {
        var hud = FindFirstObjectByType<GameHUD>();
        if (hud != null)
        {
            float t = hud.GetElapsedTime();
            survivalTimeText.text = $"Survived: {(int)(t / 60f):00}:{(int)(t % 60f):00}";
        }
        if (ResourceManager.Instance != null)
            goldEarnedText.text = $"Gold earned: {ResourceManager.Instance.GetAmount(ResourceType.Gold)}";
    }
}
