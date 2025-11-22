using UnityEngine;

namespace Flyweight.Stats
{
    [CreateAssetMenu(fileName = "ExplosionStats", menuName = "ScriptableObjects/ExplosionStats")]
    public class ExplosionProperties : ScriptableObject
    {
        [Header("Core Stats")]

        [SerializeField] private float _radius = 5f;
        public float Radius => _radius;

        [SerializeField] private float _force = 10;
        public float Force => _force;

        [SerializeField] private int _damage = 1;
        public int Damage => _damage;
    }
}