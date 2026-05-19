using UnityEngine;
using UnityEditor;
using System.IO;

public static class DamagePopupSettingsCreator
{
    [MenuItem("Tools/AgeDefense/Create Damage Popup Settings")]
    static void Create()
    {
        const string resourcesPath = "Assets/Resources";
        const string assetPath     = resourcesPath + "/DamagePopupSettings.asset";

        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.SaveAssets();
        }

        if (File.Exists(Path.Combine(Application.dataPath, "../" + assetPath)))
        {
            Debug.Log("[AgeDefense] DamagePopupSettings already exists at " + assetPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<DamagePopupSettings>(assetPath);
            return;
        }

        var settings = ScriptableObject.CreateInstance<DamagePopupSettings>();
        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
        Debug.Log("[AgeDefense] DamagePopupSettings created at " + assetPath + " — select it to edit values in the Inspector.");
    }
}
