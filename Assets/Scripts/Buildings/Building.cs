using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] protected float maxHP = 200f;
    [SerializeField] protected int buildingLevel = 1;
    [SerializeField] protected Faction faction;

    public event System.Action OnDestroyed;
    public event System.Action<float, float> OnHealthChanged;

    // Raised when a damaged building is clicked so a selected Pawn can be assigned to repair it
    public static event System.Action<Building> OnBuildingClicked;

    BuildingSlot _mySlot;   // set by BuildingSlot.Occupy() — used to free slot on destroy
    public void SetSlot(BuildingSlot slot) => _mySlot = slot;

    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public int Level => buildingLevel;
    public Faction Faction => faction;
    public bool IsFullHealth => Mathf.Approximately(CurrentHP, maxHP);

    protected virtual void Awake() => CurrentHP = maxHP;

    // For Edit Mode tests — sets HP without MonoBehaviour lifecycle
    public void InitForTest(float hp) { maxHP = hp; CurrentHP = hp; }

    public void TakeDamage(float amount)
    {
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP <= 0f) HandleDestroyed();
    }

    public void Repair(float amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    // Virtual so Castle can override without calling Destroy on itself during tests
    protected virtual void HandleDestroyed()
    {
        OnDestroyed?.Invoke();
        if (Application.isPlaying)
        {
            _mySlot?.Free();    // _mySlot set by BuildingSlot.Occupy() via SetSlot()
            Destroy(gameObject);
        }
    }

    public virtual void Upgrade()
    {
        if (buildingLevel >= 2) return;
        buildingLevel = 2;
        GetComponent<TroopSpawner>()?.OnUpgrade();
    }

    void OnMouseDown()
    {
        if (!IsFullHealth)
            OnBuildingClicked?.Invoke(this);
    }
}
