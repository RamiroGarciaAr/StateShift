using System;
using Strategies.Weapons;

namespace Commands
{
    public sealed class SwitchGunCommand : ICommand
    {
        private IGun _unEquipGun;
        private IGun _equipGun;

        private Action _onDone;

        public SwitchGunCommand(IGun equip, IGun unEquip, Action onDone)
        {
            _equipGun = equip;
            _unEquipGun = unEquip;

            _onDone = onDone;
        }

        public void Execute()
        {
            _unEquipGun.UnEquip();
            _equipGun.Equip();

            _onDone();
        }
    }
}