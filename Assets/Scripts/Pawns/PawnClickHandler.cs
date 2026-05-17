using UnityEngine;

public class PawnClickHandler : MonoBehaviour
{
    static PawnClickHandler _selected;

    Pawn _pawn;
    SpriteRenderer _renderer;
    Color _originalColor;

    void Awake()
    {
        _pawn = GetComponent<Pawn>();
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null) _originalColor = _renderer.color;
    }

    void OnMouseDown()
    {
        if (_selected != null) _selected.Deselect();
        _selected = this;
        if (_renderer != null) _renderer.color = Color.yellow;
        UIManager.Instance?.ShowPawnInfo();
        ResourceNode.OnNodeClicked   += OnNodeChosen;
        Building.OnBuildingClicked   += OnBuildingChosen;
    }

    void OnNodeChosen(ResourceNode node)
    {
        ResourceNode.OnNodeClicked  -= OnNodeChosen;
        Building.OnBuildingClicked  -= OnBuildingChosen;
        _pawn.AssignToNode(node);
        Deselect();
    }

    void OnBuildingChosen(Building building)
    {
        ResourceNode.OnNodeClicked  -= OnNodeChosen;
        Building.OnBuildingClicked  -= OnBuildingChosen;
        if (!building.IsFullHealth)
            _pawn.AssignToRepair(building);
        Deselect();
    }

    void Deselect()
    {
        if (_renderer != null) _renderer.color = _originalColor;
        UIManager.Instance?.HidePawnInfo();
        ResourceNode.OnNodeClicked  -= OnNodeChosen;
        Building.OnBuildingClicked  -= OnBuildingChosen;
        if (_selected == this) _selected = null;
    }

    void OnDestroy() => Deselect();
}
