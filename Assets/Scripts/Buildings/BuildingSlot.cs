using UnityEngine;

public class BuildingSlot : MonoBehaviour
{
    [SerializeField] int slotId;
    [SerializeField] SpriteRenderer slotIndicator;

    public bool IsOccupied { get; private set; }
    public int SlotId => slotId;

    public static event System.Action<BuildingSlot> OnSlotClicked;

    public void Occupy(GameObject buildingGO)
    {
        IsOccupied = true;
        if (slotIndicator != null) slotIndicator.enabled = false;
        BuildingManager.Instance?.OccupySlot(slotId);
        buildingGO.GetComponent<Building>()?.SetSlot(this);
    }

    public void Free()
    {
        IsOccupied = false;
        if (slotIndicator != null) slotIndicator.enabled = true;
        BuildingManager.Instance?.FreeSlot(slotId);
    }

    void OnMouseDown()
    {
        if (!IsOccupied) OnSlotClicked?.Invoke(this);
    }
}
