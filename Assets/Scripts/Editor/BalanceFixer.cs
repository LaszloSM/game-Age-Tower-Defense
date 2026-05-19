// Run via: Tools → AgeDefense → Fix Balance
// Sets Castle HP = 1000, updates building prefabs with correct HP values.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class BalanceFixer
{
    [MenuItem("Tools/AgeDefense/Fix Balance")]
    public static void FixBalance()
    {
        int changes = 0;

        // ── Castle: 1000 HP ────────────────────────────────────────────────
        string[] castlePaths = {
            "Assets/Prefabs/Buildings/BlueCastle.prefab",
            "Assets/Prefabs/Buildings/RedCastle.prefab"
        };
        foreach (var path in castlePaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { Debug.LogWarning($"[BalanceFixer] Not found: {path}"); continue; }

            var castle = prefab.GetComponent<Building>();
            if (castle == null) castle = prefab.GetComponentInChildren<Building>();
            if (castle == null) { Debug.LogWarning($"[BalanceFixer] No Building on {path}"); continue; }

            var so = new SerializedObject(castle);
            var hpProp = so.FindProperty("maxHP");
            if (hpProp != null && !Mathf.Approximately(hpProp.floatValue, 1000f))
            {
                hpProp.floatValue = 1000f;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log($"[BalanceFixer] Set {path} maxHP → 1000");
                changes++;
            }
        }

        // ── Regular buildings: 300 HP ──────────────────────────────────────
        string[] buildingPaths = {
            "Assets/Prefabs/Buildings/BlueBarracks.prefab",
            "Assets/Prefabs/Buildings/BlueArchery.prefab",
            "Assets/Prefabs/Buildings/BlueTower.prefab",
            "Assets/Prefabs/Buildings/BlueMonastery.prefab",
            "Assets/Prefabs/Buildings/BlueHouse.prefab",
            "Assets/Prefabs/Buildings/RedBarracks.prefab",
            "Assets/Prefabs/Buildings/RedArchery.prefab",
            "Assets/Prefabs/Buildings/RedTower.prefab",
            "Assets/Prefabs/Buildings/RedMonastery.prefab",
            "Assets/Prefabs/Buildings/RedHouse.prefab",
        };
        foreach (var path in buildingPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var bldg = prefab.GetComponent<Building>();
            if (bldg == null) bldg = prefab.GetComponentInChildren<Building>();
            if (bldg == null) continue;

            var so = new SerializedObject(bldg);
            var hpProp = so.FindProperty("maxHP");
            if (hpProp != null && !Mathf.Approximately(hpProp.floatValue, 300f))
            {
                hpProp.floatValue = 300f;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                changes++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BalanceFixer] Done — {changes} prefab(s) updated.");
    }
}
#endif
