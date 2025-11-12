using Commands;
using Strategies.Health;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            ICommand command = new DamageCommand(damageable, 3);
            CommandQueueManager.Instance.EnqueueCommand(command);
        }
    }
}
