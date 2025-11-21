using UnityEngine;

namespace Flyweight.Stats
{
    [CreateAssetMenu(fileName = "BulletStats", menuName = "ScriptableObjects/BulletStats")]
    public class BulletProperties : ScriptableObject
    {
        [Header("Core Stats")]

        [SerializeField] private float _flightTime = 5f;
        public float FlightTime => _flightTime;

        [SerializeField] private int _velocity = 10;
        public int Velocity => _velocity;

        [SerializeField] private bool _useGravity = true;
        public bool UseGravity => _useGravity;

        [SerializeField] private int _damage = 1;
        public int Damage => _damage;
    }
}