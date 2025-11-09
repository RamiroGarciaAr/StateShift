using Strategies.Weapons;

namespace Commands
{
    public sealed class ShootCommand : ICommand
    {
        private IGun _gun;

        public ShootCommand(IGun gun)
        {
            _gun = gun;
        }

        public void Execute()
        {
            _gun.Shoot();
        }
    }
}