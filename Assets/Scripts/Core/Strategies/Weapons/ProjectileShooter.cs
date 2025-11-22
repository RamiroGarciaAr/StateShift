using Managers;
using UnityEngine;

namespace Core.Strategies.Weapons
{
    public class ProjectileShooter : Shooter
    {
        [SerializeField] private ObjectPoolManager _projectilesPool;

        public override void Shoot(float spreadRadius)
        {
            GameObject projectile = _projectilesPool.GetPooledObject();

            Vector3 spread = Random.insideUnitSphere * spreadRadius;
            Vector3 direction = (transform.forward + spread).normalized;

            projectile.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction));
            projectile.SetActive(true);
        }
    }
}