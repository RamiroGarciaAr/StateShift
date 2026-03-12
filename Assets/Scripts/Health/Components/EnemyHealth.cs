using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public class EnemyHealth : BaseHealth
    {
        [SerializeField] private EnemyHealthConfigSO healthConfig;

        public IReadOnlyList<HealthChunk> Chunks => healthChunks;
        public int CurrentChunkIndex => GetCurrentChunkIndex();

        protected override void InitializeChunks()
        {
            healthChunks.Clear();

            if (healthConfig == null)
            {
                Debug.LogError("No health config assigned!", this);
                return;
            }

            foreach (var chunkData in healthConfig.Chunks)
            {
                healthChunks.Add(new HealthChunk(chunkData.maxHealth, chunkData.healthType));
            }
        }

        public override void Heal(float amount)
        {
            var currentChunk = healthChunks.Find(c => !c.IsDepleted);
            currentChunk?.Heal(amount);
        }

        public float GetChunkHealthNormalized(int index)
        {
            if (index < 0 || index >= healthChunks.Count) return 0f;
            return healthChunks[index].HealthNormalized;
        }
    }
}
