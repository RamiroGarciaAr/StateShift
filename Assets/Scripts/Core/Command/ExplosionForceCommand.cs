using Flyweight.Stats;
using UnityEngine;

namespace Core.Commands
{
    public sealed class ExplosionForceCommand : ICommand
    {
        private Rigidbody _rigidbody;
        private Vector3 _position;
        private ExplosionProperties _properties;

        public ExplosionForceCommand(Rigidbody rigidbody, Vector3 position, ExplosionProperties properties)
        {
            _rigidbody = rigidbody;
            _position = position;
            _properties = properties;
        }

        public void Execute()
        {
            _rigidbody.AddExplosionForce(_properties.Force, _position, _properties.Radius);
        }
    }
}