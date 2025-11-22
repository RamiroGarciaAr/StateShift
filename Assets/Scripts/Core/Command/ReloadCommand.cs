using Core.Strategies.Weapons;

namespace Core.Commands
{
    public sealed class ReloadCommand : ICommand
    {
        private IGun _gun;

        public ReloadCommand(IGun gun)
        {
            _gun = gun;
        }

        public void Execute()
        {
            _gun.Reload();
        }
    }
}