using UnityEngine;

/// <summary>
/// Updates the SpriteRenderer's sorting order every frame based on the
/// object's Y position so that objects higher on screen render behind
/// objects lower on screen (standard top-down RTS depth sorting).
///
/// Attach to every moving sprite: PlayerWarrior, Pawns, enemies.
/// Static sprites (trees, decorations) use a one-time sort set at Start.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class YSortUpdater : MonoBehaviour
{
    // Multiply Y by this to get a sorting offset.
    // 100 gives 1 unit of separation per world unit.
    [SerializeField] int sortScale = 100;

    SpriteRenderer _sr;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        // Negative Y → higher sorting order (closer to viewer)
        _sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * sortScale);
    }
}
