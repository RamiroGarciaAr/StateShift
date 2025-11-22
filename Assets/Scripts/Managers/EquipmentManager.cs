using System.Collections.Generic;
using Core.Strategies.Weapons;
using UnityEngine;

namespace Managers
{
    public sealed class EquipmentManager : MonoBehaviour
    {
        public IGun EquippedGun => _guns[EquippedGunIndex];

        private int EquippedGunIndex { get; set; } = 0;
        private int NextGunIndex => (EquippedGunIndex + 1) % _guns.Count;
        private int PreviousGunIndex => EquippedGunIndex == 0 ? _guns.Count - 1 : EquippedGunIndex - 1;

        [SerializeField] private List<Gun> _guns;

        void Start()
        {
            _guns.ForEach((gun) => gun.UnEquip());

            _guns[EquippedGunIndex].Equip();
        }

        public void SwitchNextGun()
        {
            _guns[EquippedGunIndex].UnEquip();
            _guns[NextGunIndex].Equip();

            EquippedGunIndex = NextGunIndex;
        }

        public void SwitchPreviousGun()
        {
            _guns[EquippedGunIndex].UnEquip();
            _guns[PreviousGunIndex].Equip();

            EquippedGunIndex = PreviousGunIndex;
        }
    }
}