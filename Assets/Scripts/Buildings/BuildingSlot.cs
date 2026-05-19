using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSlot : MonoBehaviour, IPointerClickHandler
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (!IsOccupied) OnSlotClicked?.Invoke(this);
    }

    void OnMouseOver()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButtonDown(1) && !IsOccupied)
            OnSlotClicked?.Invoke(this);
    }
}
