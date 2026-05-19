using UnityEngine;

/// <summary>
/// Create ONE instance of this asset and place it in any "Resources" folder:
///   Right-click in Project → Create → AgeDefense → Damage Popup Settings
/// All damage numbers in the game will read from this asset at runtime.
/// </summary>
[CreateAssetMenu(fileName = "DamagePopupSettings", menuName = "AgeDefense/Damage Popup Settings")]
public class DamagePopupSettings : ScriptableObject
{
    [Header("Font Sizes")]
    [Tooltip("Canvas pixel size of normal hit numbers.")]
    public float normalFontSize  = 70f;

    [Tooltip("Canvas pixel size of heavy-hit numbers (damage >= heavyThreshold).")]
    public float heavyFontSize   = 90f;

    [Header("Heavy Hit")]
    [Tooltip("Damage at or above this value is shown larger and in gold colour.")]
    public float heavyThreshold  = 20f;

    [Header("Animation")]
    [Tooltip("Seconds the number is visible before it fully fades out.")]
    public float popupDuration   = 0.9f;

    [Tooltip("Canvas pixels the number travels upward during its lifetime.")]
    public float floatDistance   = 70f;

    [Header("Colors")]
    public Color normalHitColor  = new Color(1f, 0.25f, 0.25f, 1f);   // red
    public Color heavyHitColor   = new Color(1f, 0.85f, 0.10f, 1f);   // gold
}
