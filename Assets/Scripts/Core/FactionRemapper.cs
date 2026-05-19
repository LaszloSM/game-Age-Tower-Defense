using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The game scene is authored assuming player = Blue (player buildings = Blue,
/// enemy buildings = Red).  When the player picks Red this swaps EVERY building's
/// faction so all logic (TroopSpawner, targeting, path-following) stays correct.
///
/// Uses SceneManager.sceneLoaded so it fires each time the Game scene loads —
/// RuntimeInitializeOnLoadMethod(AfterSceneLoad) only fires once at app start
/// (on MainMenu) and would miss the Game scene entirely.
/// </summary>
public static class FactionRemapper
{
    // Register once, at the very first moment the app runs.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Called after every scene load, AFTER all Awake() calls but BEFORE Start().
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Blue is the authored default — nothing to do.
        if (FactionSelectUI.ChosenFaction == Faction.Blue) return;

        // If there are no buildings this is not the Game scene.
        var buildings = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
        if (buildings.Length == 0) return;

        // Swap Blue ↔ Red on every building (includes Castles).
        foreach (var b in buildings)
            b.SetFaction(b.Faction == Faction.Blue ? Faction.Red : Faction.Blue);

        Debug.Log($"[FactionRemapper] Remapped {buildings.Length} buildings for Red faction in scene '{scene.name}'.");
    }
}
