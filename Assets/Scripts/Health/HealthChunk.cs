using UnityEngine;

namespace Health
{
    [System.Serializable]
    public class HealthChunk
    {
        [SerializeField] private float maxHealth;
        [SerializeField] private HealthType healthType;

        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public HealthType HealthType => healthType;
        public bool IsDepleted => _currentHealth <= 0f;
        public float HealthNormalized => maxHealth > 0 ? _currentHealth / maxHealth : 0f;

        public HealthChunk(float maxHealth, HealthType healthType)
        {
            this.maxHealth = maxHealth;
            this.healthType = healthType;
            _currentHealth = maxHealth;
        }

        public float ApplyDamage(float damage)
        {
            if (IsDepleted) return damage;

            float absorbed = Mathf.Min(damage, _currentHealth);
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            return Mathf.Max(0f, damage - absorbed);
        }

        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        }

        public void Restore()
        {
            _currentHealth = maxHealth;
        }
    }
}
