using System;
using System.Collections.Generic;
using UnityEngine;

namespace Health
{
    public class EnemyHealth : BaseHealth
    {
        [SerializeField]
        private EnemyHealthConfigSO healthConfig;

        [SerializeField]
        DamageDealtChannelSO damageDealtChannel;

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

        //We grab the event so we can use it in other things
        protected override void OnDamageApplied(in DamageDealtEvent result)
        {
            if (damageDealtChannel == null)
            {
                Debug.LogError("[EnemyHealth]DamageDealtChannel not assigned", this);
                return;
            }

            damageDealtChannel.Raise(in result);
        }

        public float GetChunkHealthNormalized(int index)
        {
            if (index < 0 || index >= healthChunks.Count)
                return 0f;
            return healthChunks[index].HealthNormalized;
        }
    }
}
