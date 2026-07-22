using Health;
using UnityEngine;

public class HealthPickup : MonoBehaviour, IPickupEffect
{
    [SerializeField]
    private AudioPool audioPool;

    [SerializeField]
    private Sound pickupSound;

    [SerializeField]
    private float healAmount = 25f;

    public bool CanApply(GameObject collector)
    {
        var health = collector.GetComponent<PlayerHealth>();
        return health != null && health.CanHeal;
    }

    public void Apply(GameObject collector)
    {
        collector.GetComponent<PlayerHealth>().Heal(healAmount);
        audioPool?.PlayAt(pickupSound, null, is3D: false);
    }
}
