using UnityEngine;

public class ParticlePrefabRegistry : MonoBehaviour
{
    [SerializeField] GameObject dustPrefab;
    [SerializeField] GameObject firePrefab;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] GameObject waterSplashPrefab;
    [SerializeField] GameObject healEffectPrefab;

    void Awake()
    {
        ParticleSpawner.Dust        = dustPrefab;
        ParticleSpawner.Fire        = firePrefab;
        ParticleSpawner.Explosion   = explosionPrefab;
        ParticleSpawner.WaterSplash = waterSplashPrefab;
        ParticleSpawner.HealEffect  = healEffectPrefab;
    }
}
