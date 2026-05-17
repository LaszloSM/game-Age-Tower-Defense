using UnityEngine;

public class HousePawnEffect : MonoBehaviour
{
    bool _registered;

    void Start()
    {
        if (_registered) return;
        ResourceManager.Instance?.IncrementPawnSlotCap();
        _registered = true;
        GetComponent<Building>().OnDestroyed += OnHouseDestroyed;
    }

    void OnHouseDestroyed()
    {
        // Pawn slots are not decremented on house loss — keeps gameplay fair
    }
}
