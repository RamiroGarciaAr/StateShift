using UnityEngine;
using Core;

namespace Strategies
{
    public interface IMovable
    {
        public float Speed { get; }
        public void SetMovementState(MovementState state);
        public void Move(Vector2 direction);
    }
}
