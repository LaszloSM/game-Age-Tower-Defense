using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, FactionSelect, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Menu;
    public static event System.Action<GameState> OnStateChanged;

    // ── Singleton bootstrap ───────────────────────────────────────────────────
    // Calling Get() guarantees an instance even if GameManager is not in the scene.
    public static GameManager Get()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();   // Awake runs immediately → sets Instance
        return Instance;
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    public void StartGame()
    {
        ChangeState(GameState.Playing);
        LoadScene("Game");
    }

    public void GoToFactionSelect()
    {
        ChangeState(GameState.FactionSelect);
        LoadScene("FactionSelect");
    }

    public void GoToMainMenu()
    {
        ChangeState(GameState.Menu);
        LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        Time.timeScale = 0f;
        ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;
        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0f;
        ChangeState(GameState.GameOver);
    }
}
