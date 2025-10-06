using UnityEngine;

namespace Strategies
{
    public interface IMovable
    {
        public float Speed { get; }
        public float SpeedMultiplier { get; set; }
        public void Move(Vector2 direction);
    }
}
