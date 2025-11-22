using UnityEngine;

namespace Core.Strategies.Weapons
{
    public abstract class Shooter : MonoBehaviour, IShooter
    {
        public abstract void Shoot(float spreadRadius);
    }
}