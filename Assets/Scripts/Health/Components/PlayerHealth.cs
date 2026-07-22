using System;
using UnityEngine;

namespace Health
{
    public class PlayerHealth : BaseHealth
    {
        [SerializeField]
        private PlayerHealthConfigSO healthConfig;

        private int _mainChunkIndex = -1;

        public float MainChunkHealthNormalized =>
            _mainChunkIndex >= 0 ? healthChunks[_mainChunkIndex].HealthNormalized : 0f;

        public float GetSideChunkHealthNormalized(int sideIndex)
        {
            if (sideIndex < 0 || sideIndex >= healthChunks.Count || sideIndex == _mainChunkIndex)
                return 0f;
            return healthChunks[sideIndex].HealthNormalized;
        }

        public int TotalSideChunks => healthConfig != null ? healthConfig.SideChunkCount : 0;

        public int ActiveSideChunks
        {
            get
            {
                int active = 0;
                for (int i = 0; i < healthChunks.Count; i++)
                {
                    if (i == _mainChunkIndex)
                        continue;
                    if (!healthChunks[i].IsDepleted)
                        active++;
                }
                return active;
            }
        }

        public bool HasLostChunks
        {
            get
            {
                for (int i = 0; i < healthChunks.Count; i++)
                    if (healthChunks[i].IsDepleted)
                        return true;
                return false;
            }
        }

        public event Action<int> OnSideChunkRestored;

        protected override void InitializeChunks()
        {
            healthChunks.Clear();
            _mainChunkIndex = -1;

            if (healthConfig == null)
            {
                Debug.LogError("No player health config assigned!", this);
                return;
            }

            // Side chunks first: they absorb damage before main health is touched.
            for (int i = 0; i < healthConfig.SideChunkCount; i++)
            {
                healthChunks.Add(
                    new HealthChunk(healthConfig.SideChunkHealth, HealthType.Player, absorbs: true)
                );
            }

            // Main chunk last: the core, only reached once the buffer is gone.
            healthChunks.Add(new HealthChunk(healthConfig.MainChunkHealth, HealthType.Player));
            _mainChunkIndex = healthChunks.Count - 1;
        }

        /*
                public override void Heal(float amount)
                {
                    // Only the main chunk heals naturally. Side chunks come back via RestoreSideChunk.
                    if (_mainChunkIndex >= 0 && !healthChunks[_mainChunkIndex].IsDepleted)
                    {
                        healthChunks[_mainChunkIndex].Heal(amount);
                    }
                }
                */

        // Interface for health items
        public void RestoreSideChunk()
        {
            for (int i = 0; i < healthChunks.Count; i++)
            {
                if (i == _mainChunkIndex)
                    continue;

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
            for (int i = 0; i < healthChunks.Count; i++)
            {
                if (i == _mainChunkIndex)
                    continue;

                if (healthChunks[i].IsDepleted)
                    return true;
            }
            return false;
        }
    }
}
