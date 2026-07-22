using Health;
using UnityEngine;

public class HealthPickup : MonoBehaviour, IPickupEffect
{
    private AudioPool _audioPool;

    [SerializeField]
    private Sound pickupSound;

    [SerializeField]
    private float healAmount = 25f;

    public void SetAudioPool(AudioPool pool) => _audioPool = pool;

    public bool CanApply(GameObject collector)
    {
        var health = collector.GetComponent<PlayerHealth>();
        return health != null && health.CanHeal;
    }

    public void Apply(GameObject collector)
    {
        collector.GetComponent<PlayerHealth>().Heal(healAmount);
        _audioPool?.PlayAt(pickupSound, null, is3D: false);
    }
}
