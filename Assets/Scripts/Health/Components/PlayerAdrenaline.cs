using System;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerAdrenaline : MonoBehaviour, IDamageModifier
    {
        [Header("Adrenaline Settings")]
        [SerializeField] private int maxLevel = 4;
        [SerializeField] private float damageReductionPerLevel = 0.10f;

        [Header("Momentum Thresholds (0-1)")]
        [SerializeField] private float[] levelThresholds = { 0.25f, 0.50f, 0.75f, 1.00f };

        private PlayerHealth _playerHealth;
        private PlayerMovement _playerMovement;
        private int _currentLevel = 0;

        // UI Properties
        public int CurrentLevel => _currentLevel;
        public int MaxLevel => maxLevel;
        public float LevelNormalized => (float)_currentLevel / maxLevel;
        public float DamageReduction => _currentLevel * damageReductionPerLevel;

        // IDamageModifier
        public int Priority => 100;

        public event Action<int> OnLevelChanged;

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Start()
        {
            _playerHealth.RegisterDamageModifier(this);
        }

        private void OnDestroy()
        {
            _playerHealth?.UnregisterDamageModifier(this);
        }

        private void Update()
        {
            UpdateAdrenalineLevel();
        }

        private void UpdateAdrenalineLevel()
        {
            if (_playerMovement == null) return;

            float momentum = _playerMovement.Momentum01;
            int newLevel = 0;

            for (int i = 0; i < levelThresholds.Length && i < maxLevel; i++)
            {
                if (momentum >= levelThresholds[i])
                {
                    newLevel = i + 1;
                }
            }

            if (newLevel != _currentLevel)
            {
                _currentLevel = newLevel;
                OnLevelChanged?.Invoke(_currentLevel);
            }
        }

        public float ModifyDamage(float baseDamage, DamageInfo damageInfo)
        {
            float reduction = 1f - DamageReduction;
            return baseDamage * reduction;
        }
    }
}
