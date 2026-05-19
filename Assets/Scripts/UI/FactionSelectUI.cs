using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to any GameObject in the FactionSelect scene (e.g. the Canvas).
/// Buttons and highlights are found automatically. Assign them in the Inspector
/// to override auto-detection.
///
/// Background fix: the first Image found whose name contains "background",
/// "fondo", or "bg" is automatically stretched to fill the whole screen.
/// You can also assign it explicitly via the backgroundImage field.
/// </summary>
public class FactionSelectUI : MonoBehaviour
{
    public static Faction ChosenFaction { get; private set; } = Faction.Blue;

    [Header("Buttons — assign in Inspector OR leave empty to auto-detect")]
    [SerializeField] Button blueCard;
    [SerializeField] Button redCard;
    [SerializeField] Button confirmButton;

    [Header("Highlights — assign OR leave empty to auto-detect")]
    [SerializeField] Image blueHighlight;
    [SerializeField] Image redHighlight;

    [Header("Background image (stretched to fill screen at runtime)")]
    [Tooltip("Drag the background Image here. If left empty the script tries to find it by name.")]
    [SerializeField] Image backgroundImage;

    static readonly Color ColBlue       = new Color(0.20f, 0.55f, 1.00f, 0.55f);
    static readonly Color ColRed        = new Color(1.00f, 0.25f, 0.20f, 0.55f);

    void Start()
    {
        FixCanvas();
        AutoWire();
        FixBackground();

        if (blueCard      != null) blueCard.onClick.AddListener(()  => SelectFaction(Faction.Blue));
        if (redCard       != null) redCard.onClick.AddListener(()   => SelectFaction(Faction.Red));
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);

        SelectFaction(ChosenFaction);
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    void SelectFaction(Faction f)
    {
        ChosenFaction = f;

        if (blueHighlight != null)
        {
            blueHighlight.color   = ColBlue;
            blueHighlight.enabled = (f == Faction.Blue);
        }
        if (redHighlight != null)
        {
            redHighlight.color   = ColRed;
            redHighlight.enabled = (f == Faction.Red);
        }
    }

    void OnConfirm() => GameManager.Get().StartGame();


    // ── Auto-wire ─────────────────────────────────────────────────────────────

    void AutoWire()
    {
        Button[] allBtns  = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Image[]  allImgs  = FindObjectsByType<Image>(FindObjectsSortMode.None);

        foreach (var btn in allBtns)
        {
            string n = btn.gameObject.name.ToLower();

            if (blueCard == null && ContainsAny(n, "blue","azul","faction_b","cardb","bluefaction"))
                blueCard = btn;

            if (redCard == null && ContainsAny(n, "red","rojo","faction_r","cardr","redfaction"))
                redCard = btn;

            if (confirmButton == null && ContainsAny(n, "confirm","confirmar","play","jugar","start","elegir","select","ok"))
                confirmButton = btn;
        }

        foreach (var img in allImgs)
        {
            string n = img.gameObject.name.ToLower();

            if (blueHighlight == null && ContainsAny(n, "bluehighlight","highlight_blue","selectedblue","selectblue","selblue"))
                blueHighlight = img;

            if (redHighlight == null && ContainsAny(n, "redhighlight","highlight_red","selectedred","selectred","selred"))
                redHighlight = img;
        }

        // Fallback warnings
        if (blueCard      == null) Debug.LogWarning("[FactionSelectUI] Blue button not found. Name it to include 'blue' or 'azul'.");
        if (redCard       == null) Debug.LogWarning("[FactionSelectUI] Red button not found. Name it to include 'red' or 'rojo'.");
        if (confirmButton == null) Debug.LogWarning("[FactionSelectUI] Confirm button not found. Name it to include 'confirm' or 'confirmar'.");
    }

    // ── Background fix ────────────────────────────────────────────────────────

    void FixBackground()
    {
        // 1. Find the background GameObject by name or by assigned field
        GameObject bgGO = backgroundImage != null ? backgroundImage.gameObject : null;

        if (bgGO == null)
        {
            string[] candidates = { "fondo","background","bg","back","backdrop","Background","Fondo","BG" };
            foreach (string n in candidates)
            {
                bgGO = GameObject.Find(n);
                if (bgGO != null) break;
            }
        }

        if (bgGO == null) { Debug.LogWarning("[FactionSelectUI] Background GameObject not found."); return; }

        // 2. If it has a SpriteRenderer (NOT a UI Image), convert it to Image at runtime
        var sr = bgGO.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Sprite sprite = sr.sprite;
            sr.enabled = false;                          // hide the SpriteRenderer

            var img = bgGO.GetComponent<Image>();
            if (img == null) img = bgGO.AddComponent<Image>(); // CanvasRenderer auto-added by Unity
            img.sprite          = sprite;
            img.type            = Image.Type.Simple;
            img.preserveAspect  = false;
            img.raycastTarget   = false;
            backgroundImage     = img;
        }

        // 3. Stretch the RectTransform to fill the entire Canvas
        if (backgroundImage != null)
        {
            var rt            = backgroundImage.rectTransform;
            rt.anchorMin      = Vector2.zero;
            rt.anchorMax      = Vector2.one;
            rt.offsetMin      = Vector2.zero;
            rt.offsetMax      = Vector2.zero;
            rt.pivot          = new Vector2(0.5f, 0.5f);
            rt.localScale     = Vector3.one;             // reset any accidental scale (was 50,50,1)
            bgGO.transform.SetAsFirstSibling();          // push behind all other UI elements
        }
    }

    // ── Canvas fix ────────────────────────────────────────────────────────────

    void FixCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // ScreenSpaceOverlay renders correctly on any aspect ratio without a camera
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    static bool ContainsAny(string source, params string[] keywords)
    {
        foreach (var k in keywords)
            if (source.Contains(k)) return true;
        return false;
    }
}
