using System;
using UnityEngine;

namespace Core.Strategies.Health
{
    public class PropHealth : MonoBehaviour, IHealth, IDamageable
    {
        public int Health => _health;
        public int MaxHealth => _maxHealth;

        [SerializeField] private int _maxHealth;
        [SerializeField] private int _health;

        private void Start()
        {
            _health = _maxHealth;
        }

        public void Damage(int damage)
        {
            _health = Math.Max(0, _health - damage);

            if (_health == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}