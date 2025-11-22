using Managers;

namespace Core.Commands
{
    public sealed class SwitchPreviousGunCommand : ICommand
    {
        private EquipmentManager _equipmentManager;

        public SwitchPreviousGunCommand(EquipmentManager equipmentManager)
        {
            _equipmentManager = equipmentManager;
        }

        public void Execute()
        {
            _equipmentManager.SwitchPreviousGun();
        }
    }
}