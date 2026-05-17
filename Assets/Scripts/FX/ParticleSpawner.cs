using UnityEngine;

public static class ParticleSpawner
{
    public static GameObject Dust;
    public static GameObject Fire;
    public static GameObject Explosion;
    public static GameObject WaterSplash;
    public static GameObject HealEffect;

    public static void Spawn(GameObject prefab, Vector3 position, float duration = 2f)
    {
        if (prefab == null) return;
        var go = Object.Instantiate(prefab, position, Quaternion.identity);
        Object.Destroy(go, duration);
    }
}
