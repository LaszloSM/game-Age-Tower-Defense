using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] Image fillImage;
    Unit _unit;
    Building _building;

    void Start()
    {
        _unit = GetComponentInParent<Unit>();
        _building = GetComponentInParent<Building>();

        if (_unit != null) _unit.OnDied += () => gameObject.SetActive(false);
        if (_building != null) _building.OnDestroyed += () => gameObject.SetActive(false);
    }

    void Update()
    {
        if (_unit != null && !_unit.IsDead)
            fillImage.fillAmount = _unit.CurrentHP / _unit.MaxHP;
        else if (_building != null)
            fillImage.fillAmount = _building.CurrentHP / _building.MaxHP;
    }
}
