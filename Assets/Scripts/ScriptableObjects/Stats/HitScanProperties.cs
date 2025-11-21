using UnityEngine;

namespace Flyweight.Stats
{
    [CreateAssetMenu(fileName = "HitScanStats", menuName = "ScriptableObjects/HitScanStats")]
    public class HitScanProperties : ScriptableObject
    {
        [Header("Core Stats")]

        [SerializeField] private int _damage = 1;
        public int Damage => _damage;

        [SerializeField] private float _range = Mathf.Infinity;
        public float Range => _range;
    }
}