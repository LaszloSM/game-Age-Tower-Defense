using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to any GameObject in the MainMenu scene (e.g. the Canvas).
/// Buttons are found automatically by scanning all Button children —
/// matched by lowercase name keywords. Assign them in the Inspector to
/// override the auto-detection.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons — assign in Inspector OR leave empty to auto-detect")]
    [SerializeField] Button playButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button optionsButton;

    [Header("Optional")]
    [SerializeField] GameObject optionsPanel;
    [SerializeField] Texture2D  cursorTexture;

    void Start()
    {
        AutoWire();

        if (playButton    != null) playButton.onClick.AddListener(OnPlay);
        if (quitButton    != null) quitButton.onClick.AddListener(OnQuit);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptions);

        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);

        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
    }

    // ── Auto-wire: scans all Button children of the root Canvas ──────────────

    void AutoWire()
    {
        // Search the entire scene hierarchy, not just our own children
        Button[] all = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (var btn in all)
        {
            string n = btn.gameObject.name.ToLower();

            if (playButton == null && ContainsAny(n, "play","jugar","start","comenzar","inicio"))
                playButton = btn;

            if (quitButton == null && ContainsAny(n, "quit","salir","exit","cerrar","abandonar"))
                quitButton = btn;

            if (optionsButton == null && ContainsAny(n, "option","setting","config","ajuste","opciones"))
                optionsButton = btn;
        }

        if (playButton == null)
            Debug.LogWarning("[MainMenuUI] Play button not found. Name it 'PlayButton' or assign it in the Inspector.");
        if (quitButton == null)
            Debug.LogWarning("[MainMenuUI] Quit button not found. Name it 'QuitButton' or assign it in the Inspector.");
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    void OnPlay()    => GameManager.Get().GoToFactionSelect();
    void OnOptions() => optionsPanel?.SetActive(!optionsPanel.activeSelf);

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static bool ContainsAny(string source, params string[] keywords)
    {
        foreach (var k in keywords)
            if (source.Contains(k)) return true;
        return false;
    }
}
