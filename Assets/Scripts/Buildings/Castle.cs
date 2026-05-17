using UnityEngine;

public class Castle : Building
{
    [SerializeField] ParticleSystem fireEffect;
    [SerializeField] float fireThreshold = 0.30f;

    bool _firePlaying;

    protected override void Awake()
    {
        base.Awake();
        OnHealthChanged += CheckFireThreshold;
    }

    void CheckFireThreshold(float current, float max)
    {
        bool shouldFire = current / max <= fireThreshold && current > 0f;
        if (shouldFire && !_firePlaying)  { fireEffect?.Play(); _firePlaying = true; }
        else if (!shouldFire && _firePlaying) { fireEffect?.Stop(); _firePlaying = false; }
    }

    protected override void HandleDestroyed()
    {
        fireEffect?.Stop();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.gameOver);
        ParticleSpawner.Spawn(ParticleSpawner.Explosion, transform.position, 3f);
        GameManager.Instance?.TriggerGameOver();
        // Castle is not destroyed from scene — GameOver panel covers it
    }

    public void EmergencyRepair()
    {
        if (ResourceManager.Instance == null) return;
        if (!ResourceManager.Instance.Spend(ResourceType.Gold, 100)) return;
        Repair(100f);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buildingPlace);
    }
}
