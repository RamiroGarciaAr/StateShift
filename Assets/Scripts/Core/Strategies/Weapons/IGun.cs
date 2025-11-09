using System;

namespace Strategies.Weapons
{
    public interface IGun
    {
        GunProperties Properties { get; }

        int AmmoOnMagazine { get; }
        int AmmoOnReserve { get; }

        event Action OnShot;
        event Action OnReloaded;

        void Shoot();
        void Reload();
        void Equip();
        void UnEquip();
    }
}