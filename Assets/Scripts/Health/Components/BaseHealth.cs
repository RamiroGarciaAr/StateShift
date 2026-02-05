using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Health
{
    public abstract class BaseHealth : MonoBehaviour, IHealth, IDamagable, IHealable
    {
        [SerializeField] protected DamageMatrixSO damageMatrix;

        protected List<HealthChunk> healthChunks = new();
        protected List<IDamageModifier> damageModifiers = new();

        public float CurrentHealth => healthChunks.Sum(c => c.CurrentHealth);
        public float MaxHealth => healthChunks.Sum(c => c.MaxHealth);
        public float HealthNormalize => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        public bool isAlive => CurrentHealth > 0f;
        public virtual bool CanHeal => isAlive && CurrentHealth < MaxHealth;

        public event Action<HealthChangeEventArgs> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int> OnChunkDepleted;

        protected virtual void Awake()
        {
            InitializeChunks();
        }

        protected abstract void InitializeChunks();

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (!isAlive) return;

            float previousHealth = CurrentHealth;

            // Apply damage modifiers (adrenaline, armor, etc.)
            float modifiedDamage = ApplyDamageModifiers(damageInfo);
            damageInfo.FinalDamage = modifiedDamage;

            // Flow damage through chunks
            float remainingDamage = modifiedDamage;
            for (int i = 0; i < healthChunks.Count && remainingDamage > 0; i++)
            {
                if (healthChunks[i].IsDepleted) continue;

                // Get multiplier from DamageMatrix SO
                float multiplier = damageMatrix != null ? damageMatrix.GetMultiplier(damageInfo.DamageType, healthChunks[i].HealthType): 1f;

                bool wasDepletedBefore = healthChunks[i].IsDepleted;
                remainingDamage = healthChunks[i].ApplyDamage(remainingDamage * multiplier);

                if (!wasDepletedBefore && healthChunks[i].IsDepleted)
                {
                    OnChunkDepleted?.Invoke(i);
                }
            }

            float damageDealt = previousHealth - CurrentHealth;
            var args = new HealthChangeEventArgs(
                previousHealth, CurrentHealth, MaxHealth,
                damageDealt, GetCurrentChunkIndex(), false
            );
            OnHealthChanged?.Invoke(args);

            if (!isAlive)
            {
                OnDeath?.Invoke();
            }
        }

        protected float ApplyDamageModifiers(DamageInfo damageInfo)
        {
            float damage = damageInfo.BaseDamage;
            var sortedModifiers = damageModifiers.OrderBy(m => m.Priority);

            foreach (var modifier in sortedModifiers)
            {
                damage = modifier.ModifyDamage(damage, damageInfo);
            }

            return damage;
        }

        public void RegisterDamageModifier(IDamageModifier modifier)
        {
            if (!damageModifiers.Contains(modifier))
                damageModifiers.Add(modifier);
        }

        public void UnregisterDamageModifier(IDamageModifier modifier)
        {
            damageModifiers.Remove(modifier);
        }

        protected int GetCurrentChunkIndex()
        {
            for (int i = 0; i < healthChunks.Count; i++)
            {
                if (!healthChunks[i].IsDepleted) return i;
            }
            return healthChunks.Count - 1;
        }

        public abstract void Heal(float amount);
    }
}
