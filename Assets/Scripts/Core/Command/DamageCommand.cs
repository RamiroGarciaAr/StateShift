using Strategies.Health;

namespace Commands
{
    public sealed class DamageCommand : ICommand
    {
        private IDamageable _damageable;
        private int _damage;

        public DamageCommand(IDamageable damageable, int damage)
        {
            _damageable = damageable;
            _damage = damage;
        }

        public void Execute()
        {
            _damageable.Damage(_damage);
        }
    }
}