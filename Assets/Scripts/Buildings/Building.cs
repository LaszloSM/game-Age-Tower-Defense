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

    GameObject _fireInstance;

    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public int Level => buildingLevel;
    public Faction Faction => faction;

    /// <summary>Called by FactionRemapper at game-start to mirror factions when player picks Red.</summary>
    public void SetFaction(Faction f) => faction = f;
    public bool IsFullHealth => Mathf.Approximately(CurrentHP, maxHP);

    protected virtual void Awake() => CurrentHP = maxHP;

    // For Edit Mode tests — sets HP without MonoBehaviour lifecycle
    public void InitForTest(float hp) { maxHP = hp; CurrentHP = hp; }

    public void TakeDamage(float amount)
    {
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        DamagePopup.Create(transform.position, amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        UpdateFireEffect();
        if (CurrentHP <= 0f) HandleDestroyed();
    }

    public void Repair(float amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        UpdateFireEffect();
    }

    protected virtual void UpdateFireEffect()
    {
        bool shouldBurn = CurrentHP > 0f && (CurrentHP / maxHP) <= 0.25f;
        if (shouldBurn && _fireInstance == null && ParticleSpawner.Fire != null)
        {
            _fireInstance = Object.Instantiate(ParticleSpawner.Fire, transform.position, Quaternion.identity);
            _fireInstance.transform.SetParent(transform);
        }
        else if (!shouldBurn && _fireInstance != null)
        {
            Object.Destroy(_fireInstance);
            _fireInstance = null;
        }
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
