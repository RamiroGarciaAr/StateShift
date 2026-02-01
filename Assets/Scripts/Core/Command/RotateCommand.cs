using UnityEngine;

namespace Core.Commands
{
    public sealed class RotateCommand : ICommand
    {
        private readonly Transform _target;
        private readonly Quaternion _rotation;

        public RotateCommand(Transform target, Quaternion rotation)
        {
            _target = target;
            _rotation = rotation;
        }

        public void Execute()
        {
            _target.rotation = _rotation;
        }
    }
}