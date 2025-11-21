using Commands;
using Flyweight.Stats;
using Strategies.Health;
using UnityEngine;

namespace Strategies.Weapons
{
    public class HitScanShooter : Shooter
    {
        [SerializeField] private HitScanProperties _properties;

        public HitScanProperties Properties => _properties;

        public override void Shoot(float spreadRadius)
        {
            Vector3 spread = Random.insideUnitSphere * spreadRadius;
            Vector3 direction = (transform.forward + spread).normalized;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, Properties.Range))
            {
                if (hitInfo.collider.TryGetComponent(out IDamageable damageable))
                {
                    ICommand command = new DamageCommand(damageable, Properties.Damage);
                    CommandQueueManager.Instance.EnqueueCommand(command);
                }
            }
        }
    }
}