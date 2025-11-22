using UnityEngine;

namespace Core.Strategies.Weapons
{
    public class ShotgunShooter : HitScanShooter
    {
        [SerializeField] private int _pellets = 6;

        public override void Shoot(float spreadRadius)
        {
            for (int i = 0; i < _pellets; i++)
            {
                base.Shoot(spreadRadius);
            }
        }
    }
}