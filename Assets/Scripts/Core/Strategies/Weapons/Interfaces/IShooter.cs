using UnityEngine;

namespace Strategies.Weapons
{
    public interface IShooter
    {
        void Shoot(float spreadRadius);
    }
}