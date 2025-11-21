using System;
using Flyweight.Stats;

namespace Strategies.Weapons
{
    public interface IGun
    {
        GunProperties Properties { get; }

        int AmmoOnMagazine { get; }
        int AmmoOnReserve { get; }

        void Shoot();
        void Reload();
        void Equip();
        void UnEquip();
    }
}