using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] Texture2D cursorTexture;

    void Start()
    {
        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);

        playButton.onClick.AddListener(OnPlay);
        optionsButton.onClick.AddListener(OnOptions);
        quitButton.onClick.AddListener(OnQuit);
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
    }

    void OnPlay() => GameManager.Instance?.GoToFactionSelect();

    void OnOptions() => optionsPanel?.SetActive(!optionsPanel.activeSelf);

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
