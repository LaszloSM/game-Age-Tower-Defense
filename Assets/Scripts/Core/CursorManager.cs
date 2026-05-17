using UnityEngine;

/// <summary>
/// Manages custom hardware cursors.
/// Attach to a persistent GameObject in the Game scene.
/// Wire Cursor_02 (hand) as interactCursor and Cursor_03 (cancel) as cancelCursor.
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] Texture2D defaultCursor;
    [SerializeField] Texture2D interactCursor;   // Cursor_02 — shown over available resource nodes
    [SerializeField] Texture2D cancelCursor;     // Cursor_03 — shown over already-assigned nodes

    // Hotspot = the "click point" within the texture (in pixels from top-left)
    [SerializeField] Vector2 interactHotspot = new Vector2(8, 4);
    [SerializeField] Vector2 cancelHotspot   = new Vector2(16, 16);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SetDefault();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void SetDefault()  => Cursor.SetCursor(defaultCursor,  Vector2.zero,    CursorMode.Auto);
    public void SetInteract() => Cursor.SetCursor(interactCursor, interactHotspot, CursorMode.Auto);
    public void SetCancel()   => Cursor.SetCursor(cancelCursor,   cancelHotspot,   CursorMode.Auto);
}
