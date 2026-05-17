using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] Button resumeButton;
    [SerializeField] Button menuButton;

    void Start()
    {
        resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        menuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
    }
}
