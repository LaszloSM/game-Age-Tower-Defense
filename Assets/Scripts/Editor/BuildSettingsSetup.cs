using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Adds the three game scenes to Build Settings automatically.
/// Run once: Tools → AgeDefense → Setup Build Settings
/// </summary>
public static class BuildSettingsSetup
{
    static readonly string[] GameScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/FactionSelect.unity",
        "Assets/Scenes/Game.unity",
    };

    [MenuItem("Tools/AgeDefense/Setup Build Settings")]
    public static void Setup()
    {
        var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        foreach (string path in GameScenes)
        {
            bool found = false;
            foreach (var s in existing)
                if (s.path == path) { found = true; break; }

            if (!found)
            {
                existing.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"[BuildSetup] Added to Build Settings: {path}");
                changed = true;
            }
            else
            {
                // Make sure it's enabled
                for (int i = 0; i < existing.Count; i++)
                {
                    if (existing[i].path == path && !existing[i].enabled)
                    {
                        existing[i] = new EditorBuildSettingsScene(path, true);
                        Debug.Log($"[BuildSetup] Re-enabled in Build Settings: {path}");
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = existing.ToArray();
            Debug.Log("[BuildSetup] Build Settings updated successfully.");
        }
        else
        {
            Debug.Log("[BuildSetup] All scenes already present in Build Settings.");
        }
    }
}
