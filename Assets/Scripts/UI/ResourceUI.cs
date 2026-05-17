using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD panel that displays current Wood / Gold / Food counts.
/// Attach to the ResourcePanel GameObject inside the HUD Canvas.
/// Wire woodIcon/goldIcon/foodIcon (UI Image) and the three TMP texts.
/// </summary>
public class ResourceUI : MonoBehaviour
{
    [Header("Wood")]
    [SerializeField] Image    woodIcon;
    [SerializeField] TMP_Text woodText;

    [Header("Gold")]
    [SerializeField] Image    goldIcon;
    [SerializeField] TMP_Text goldText;

    [Header("Food")]
    [SerializeField] Image    foodIcon;
    [SerializeField] TMP_Text foodText;

    void OnEnable()
    {
        ResourceManager.OnResourceChanged += Refresh;
        Refresh();
    }

    void OnDisable() => ResourceManager.OnResourceChanged -= Refresh;

    void Refresh()
    {
        if (ResourceManager.Instance == null) return;
        if (woodText != null) woodText.text = ResourceManager.Instance.GetAmount(ResourceType.Wood).ToString();
        if (goldText != null) goldText.text = ResourceManager.Instance.GetAmount(ResourceType.Gold).ToString();
        if (foodText != null) foodText.text = ResourceManager.Instance.GetAmount(ResourceType.Food).ToString();
    }
}
