using UnityEngine;

namespace Strategies.Weapons
{
    public class Shotgun : Gun, IGun
    {
        [SerializeField] private int _pellets = 6;

        protected override void FireBullet()
        {
            for (int i = 0; i < _pellets; i++)
            {
                base.FireBullet();
            }
        }
    }
}