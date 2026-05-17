using UnityEngine;

public class PauseInputHandler : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        var gm = GameManager.Instance;
        if (gm == null) return;
        if (gm.State == GameState.Playing) gm.PauseGame();
        else if (gm.State == GameState.Paused) gm.ResumeGame();
    }
}
