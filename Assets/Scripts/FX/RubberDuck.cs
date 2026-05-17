using UnityEngine;

public class RubberDuck : MonoBehaviour
{
    void OnMouseDown()
    {
        ParticleSpawner.Spawn(ParticleSpawner.WaterSplash, transform.position, 1.5f);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.resourceCollect);
    }
}
