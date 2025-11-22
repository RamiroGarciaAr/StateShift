using Flyweight.Stats;
using UnityEngine;

namespace Core.Strategies.Weapons
{
    public interface IProjectile
    {
        ProjectileProperties Properties { get; }

        void OnProjectileCollision(Collision collision);
        void OnProjectileStop();
    }
}