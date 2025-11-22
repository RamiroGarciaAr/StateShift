using Core.Commands;
using Core.Strategies.Health;
using Flyweight.Stats;
using UnityEngine;

namespace Core.Strategies.Weapons
{
    public class Rocket : Projectile
    {
        [SerializeField] private ExplosionProperties _explosionProperties;

        public override void OnProjectileCollision(Collision collision)
        {
            Explode();
        }

        public override void OnProjectileStop()
        {
            Explode();
        }

        private void Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionProperties.Radius);

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out Rigidbody rigidbody))
                {
                    ExplosionForceCommand command = new(rigidbody, transform.position, _explosionProperties);
                    CommandQueueManager.Instance.EnqueueCommand(command);
                }

                if (collider.TryGetComponent(out IDamageable damageable))
                {
                    DamageCommand command = new(damageable, _explosionProperties.Damage);
                    CommandQueueManager.Instance.EnqueueCommand(command);
                }
            }
        }
    }
}