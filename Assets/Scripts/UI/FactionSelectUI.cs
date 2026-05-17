using UnityEngine;
using UnityEngine.UI;

public class FactionSelectUI : MonoBehaviour
{
    public static Faction ChosenFaction { get; private set; } = Faction.Blue;

    [SerializeField] Button blueCard;
    [SerializeField] Button redCard;
    [SerializeField] Button confirmButton;
    [SerializeField] Image blueHighlight;
    [SerializeField] Image redHighlight;

    void Start()
    {
        blueCard.onClick.AddListener(() => SelectFaction(Faction.Blue));
        redCard.onClick.AddListener(() => SelectFaction(Faction.Red));
        confirmButton.onClick.AddListener(OnConfirm);
        SelectFaction(Faction.Blue);
    }

    void SelectFaction(Faction f)
    {
        ChosenFaction = f;
        blueHighlight.enabled = f == Faction.Blue;
        redHighlight.enabled  = f == Faction.Red;
    }

    void OnConfirm() => GameManager.Instance?.StartGame();
}
