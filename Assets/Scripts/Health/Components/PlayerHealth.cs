using System;
using System.Linq;
using UnityEngine;

namespace Health
{
    public class PlayerHealth : BaseHealth
    {
        [SerializeField] private PlayerHealthConfigSO healthConfig;

        // UI properties (following Momentum01 pattern)
        public float MainChunkHealth01 => healthChunks.Count > 0 ? healthChunks[0].HealthNormalized : 0f;
        public int ActiveSideChunks => healthChunks.Skip(1).Count(c => !c.IsDepleted);
        public int TotalSideChunks => healthConfig != null ? healthConfig.SideChunkCount : 0;
        public bool HasLostChunks => healthChunks.Any(c => c.IsDepleted);

        public event Action<int> OnSideChunkRestored;

        protected override void InitializeChunks()
        {
            healthChunks.Clear();

            if (healthConfig == null)
            {
                Debug.LogError("No player health config assigned!", this);
                return;
            }

            // Main chunk (100 HP, no weakness)
            healthChunks.Add(new HealthChunk(healthConfig.MainChunkHealth, HealthType.Player));

            // Side chunks (20 HP each, no weakness)
            for (int i = 0; i < healthConfig.SideChunkCount; i++)
            {
                healthChunks.Add(new HealthChunk(healthConfig.SideChunkHealth, HealthType.Player));
            }
        }

        public override void Heal(float amount)
        {
            // Only heal main chunk naturally
            if (healthChunks.Count > 0 && !healthChunks[0].IsDepleted)
            {
                healthChunks[0].Heal(amount);
            }
        }

        // Interface for health items
        public void RestoreSideChunk()
        {
            for (int i = 1; i < healthChunks.Count; i++)
            {
                if (healthChunks[i].IsDepleted)
                {
                    healthChunks[i].Restore();
                    OnSideChunkRestored?.Invoke(i);
                    return;
                }
            }
        }

        public bool CanRestoreSideChunk()
        {
            return healthChunks.Skip(1).Any(c => c.IsDepleted);
        }
    }
}
