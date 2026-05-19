using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Spawns a floating damage number at a world position.
/// Call DamagePopup.Create(worldPos, damage) from anywhere.
///
/// To customise values: right-click in Project → Create → AgeDefense → Damage Popup Settings,
/// save the asset inside a "Resources" folder, then tweak it freely in the Inspector.
/// If no settings asset is found, built-in defaults are used.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    // ── Settings (loaded from Resources/DamagePopupSettings, falls back to defaults) ──
    static DamagePopupSettings _cfg;
    static DamagePopupSettings Cfg
    {
        get
        {
            if (_cfg == null)
                _cfg = Resources.Load<DamagePopupSettings>("DamagePopupSettings");
            if (_cfg == null)
                _cfg = ScriptableObject.CreateInstance<DamagePopupSettings>(); // in-memory defaults
            return _cfg;
        }
    }

    static Canvas s_Canvas;

    /// <summary>Spawn a damage number that floats up and fades out.</summary>
    public static void Create(Vector3 worldPos, float damage)
    {
        EnsureCanvas();
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 screenPos = (Vector2)cam.WorldToScreenPoint(worldPos + Vector3.up * 0.4f);

        var go = new GameObject("DmgPopup");
        go.transform.SetParent(s_Canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = ScreenToCanvasPos(screenPos);
        rt.sizeDelta = new Vector2(80, 36);

        bool isHeavy = damage >= Cfg.heavyThreshold;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = Mathf.RoundToInt(damage).ToString();
        tmp.fontSize      = isHeavy ? Cfg.heavyFontSize : Cfg.normalFontSize;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.color         = isHeavy ? Cfg.heavyHitColor : Cfg.normalHitColor;

        var popup = go.AddComponent<DamagePopup>();
        popup._tmp = tmp;
        popup._rt  = rt;
        popup.StartCoroutine(popup.Animate());
    }

    // ── Instance fields ─────────────────────────────────────────────────────

    TextMeshProUGUI _tmp;
    RectTransform   _rt;

    IEnumerator Animate()
    {
        float   t        = 0f;
        float   dur      = Cfg.popupDuration;
        float   floatPx  = Cfg.floatDistance;
        Vector2 startPos = _rt.anchoredPosition;

        while (t < dur)
        {
            t += Time.deltaTime;
            float ratio = t / dur;

            _rt.anchoredPosition = startPos + Vector2.up * (ratio * floatPx);

            float alpha = ratio < 0.5f ? 1f : 1f - (ratio - 0.5f) * 2f;
            var c = _tmp.color;
            _tmp.color = new Color(c.r, c.g, c.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    // ── Canvas bootstrap ────────────────────────────────────────────────────

    static void EnsureCanvas()
    {
        if (s_Canvas != null) return;

        var go = new GameObject("DamageCanvas");
        s_Canvas = go.AddComponent<Canvas>();
        s_Canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        s_Canvas.sortingOrder = 100;

        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    static Vector2 ScreenToCanvasPos(Vector2 screenPos)
    {
        if (s_Canvas == null) return screenPos;
        var rt = s_Canvas.GetComponent<RectTransform>();
        if (rt == null) return screenPos;

        float x = (screenPos.x / Screen.width  - 0.5f) * rt.sizeDelta.x;
        float y = (screenPos.y / Screen.height - 0.5f) * rt.sizeDelta.y;
        return new Vector2(x, y);
    }
}
