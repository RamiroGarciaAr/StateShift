using Managers;

namespace Core.Commands
{
    public sealed class SwitchNextGunCommand : ICommand
    {
        private EquipmentManager _equipmentManager;

        public SwitchNextGunCommand(EquipmentManager equipmentManager)
        {
            _equipmentManager = equipmentManager;
        }

        public void Execute()
        {
            _equipmentManager.SwitchNextGun();
        }
    }
}