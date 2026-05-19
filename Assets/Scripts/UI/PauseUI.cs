using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] Button resumeButton;
    [SerializeField] Button menuButton;

    void Start()
    {
        resumeButton.onClick.AddListener(Resume);
        menuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance?.GoToMainMenu();
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Resume();
    }

    void Resume() => GameManager.Instance?.ResumeGame();
}
