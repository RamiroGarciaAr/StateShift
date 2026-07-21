using System;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerAdrenaline : MonoBehaviour, IDamageModifier
    {
        private const float MinimumSpeedSeparation = 0.01f;
        private const float DefaultMaximumMeterValue = 1f;
        private const float DefaultMinimumSpeedForGain = 2f;
        private const float DefaultSpeedForMaximumGain = 12f;
        private const float DefaultMaximumSpeedFillPerSecond = 0.25f;
        private const float DefaultKillFillAmount = 0.25f;
        private const float DefaultDamageDrainPerDamage = 0.02f;
        private const float DefaultDisengagementThreshold = 4f;
        private const float DefaultPassiveDrainPerSecond = 0.05f;

        [Header("Speed Fill")]
        [Tooltip("Horizontal locomotion speed required before continuous adrenaline gain begins.")]
        [SerializeField, Min(0f)] private float _minimumSpeedForGain = DefaultMinimumSpeedForGain;

        [Tooltip("Horizontal locomotion speed mapped to the end of the speed fill curve.")]
        [SerializeField, Min(MinimumSpeedSeparation)] private float _speedForMaximumGain = DefaultSpeedForMaximumGain;

        [Tooltip("Maximum adrenaline gained per second before the speed fill curve multiplier is applied.")]
        [SerializeField, Min(0f)] private float _maximumSpeedFillPerSecond = DefaultMaximumSpeedFillPerSecond;

        [Tooltip("Convex normalized speed-to-fill multiplier. Its restrained terminal ramp makes the final tier require sustained peak movement.")]
        [SerializeField] private AnimationCurve _speedFillCurve = CreateDefaultSpeedFillCurve();

        [Header("Burst And Drain")]
        [Tooltip("Fixed adrenaline added for every confirmed kill.")]
        [SerializeField, Min(0f)] private float _killFillAmount = DefaultKillFillAmount;

        [Tooltip("Adrenaline removed for each point of damage received after other damage modifiers.")]
        [SerializeField, Min(0f)] private float _damageDrainPerDamage = DefaultDamageDrainPerDamage;

        [Tooltip("Seconds without qualifying locomotion or a kill before passive drain begins.")]
        [SerializeField, Min(0f)] private float _disengagementThreshold = DefaultDisengagementThreshold;

        [Tooltip("Adrenaline removed per second while disengaged.")]
        [SerializeField, Min(0f)] private float _passiveDrainPerSecond = DefaultPassiveDrainPerSecond;

        [Header("Damage Reduction")]
        [Tooltip("Damage reduction by tier. Index zero is the baseline tier and must remain zero.")]
        [SerializeField] private float[] _damageReductionPerTier = { 0f, 0.1f, 0.2f, 0.3f, 0.4f };

        [Header("Event References")]
        [Tooltip("Combat event channel that publishes confirmed enemy kills.")]
        [SerializeField] private DamageDealtChannelSO _damageDealtChannel;

        private PlayerHealth _playerHealth;
        private PlayerMovement _playerMovement;
        private float _adrenalineMeter;
        private float _disengagementElapsed;
        private int _currentLevel;
        private bool _hasWarnedAboutMissingMovement;

        public int CurrentLevel => _currentLevel;
        public int MaxLevel => Mathf.Max(0, (_damageReductionPerTier?.Length ?? 0) - 1);
        public float AdrenalineNormalized => _adrenalineMeter;
        public float DamageReduction => GetDamageReduction();
        public int Priority => 100;

        public event Action<int> OnLevelChanged;

        private void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Start()
        {
            if (_playerHealth != null)
            {
                _playerHealth.RegisterDamageModifier(this);
                _playerHealth.OnHealthChanged += HandleHealthChanged;
            }

            if (_damageDealtChannel != null)
            {
                _damageDealtChannel.OnRaised += HandleDamageDealt;
            }
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
            {
                _playerHealth.UnregisterDamageModifier(this);
                _playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            if (_damageDealtChannel != null)
            {
                _damageDealtChannel.OnRaised -= HandleDamageDealt;
            }
        }

        private void Update()
        {
            UpdateContinuousMeter(Time.deltaTime);
            UpdateTier();
        }

        /// <summary>
        /// Applies the configured fixed kill reward and resets the disengagement timer.
        /// </summary>
        public void AddKillAdrenaline()
        {
            AddToMeter(_killFillAmount);
            ResetDisengagement();
        }

        /// <summary>
        /// Gets the damage reduction assigned to the current adrenaline tier.
        /// </summary>
        public float GetDamageReduction()
        {
            if (_damageReductionPerTier == null || _damageReductionPerTier.Length == 0)
            {
                return 0f;
            }

            int tierIndex = Mathf.Clamp(_currentLevel, 0, _damageReductionPerTier.Length - 1);
            return Mathf.Clamp01(_damageReductionPerTier[tierIndex]);
        }

        /// <summary>
        /// Applies this adrenaline tier's reduction to incoming damage.
        /// </summary>
        public float ModifyDamage(float baseDamage, DamageInfo damageInfo)
        {
            return baseDamage * (1f - GetDamageReduction());
        }

        private void UpdateContinuousMeter(float deltaTime)
        {
            if (_playerMovement == null || _playerMovement.Rigidbody == null)
            {
                WarnMissingMovementOnce();
                UpdatePassiveDrain(deltaTime);
                return;
            }

            Vector3 velocity = _playerMovement.Rigidbody.velocity;
            float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            if (horizontalSpeed > _minimumSpeedForGain)
            {
                float normalizedSpeed = Mathf.Clamp01(horizontalSpeed / _speedForMaximumGain);
                float curveMultiplier = Mathf.Clamp01(_speedFillCurve.Evaluate(normalizedSpeed));
                AddToMeter(curveMultiplier * _maximumSpeedFillPerSecond * deltaTime);
                ResetDisengagement();
                return;
            }

            UpdatePassiveDrain(deltaTime);
        }

        private void UpdatePassiveDrain(float deltaTime)
        {
            _disengagementElapsed += deltaTime;
            if (_disengagementElapsed >= _disengagementThreshold)
            {
                AddToMeter(-_passiveDrainPerSecond * deltaTime);
            }
        }

        private void HandleHealthChanged(HealthChangeEventArgs healthChange)
        {
            if (healthChange.DamageDealt > 0f)
            {
                AddToMeter(-healthChange.DamageDealt * _damageDrainPerDamage);
            }
        }

        private void HandleDamageDealt(DamageDealtEvent damageDealt)
        {
            if (damageDealt.isKillShot)
            {
                AddKillAdrenaline();
            }
        }

        private void AddToMeter(float amount)
        {
            _adrenalineMeter = Mathf.Clamp01(_adrenalineMeter + amount);
        }

        private void ResetDisengagement()
        {
            _disengagementElapsed = 0f;
        }

        private void UpdateTier()
        {
            int tierCount = MaxLevel;
            int newLevel = tierCount > 0
                ? Mathf.Clamp(Mathf.FloorToInt(_adrenalineMeter * (tierCount + 1)), 0, tierCount)
                : 0;

            if (newLevel == _currentLevel)
            {
                return;
            }

            _currentLevel = newLevel;
            OnLevelChanged?.Invoke(_currentLevel);
        }

        private void WarnMissingMovementOnce()
        {
            if (_hasWarnedAboutMissingMovement)
            {
                return;
            }

            _hasWarnedAboutMissingMovement = true;
            Debug.LogWarning("[PlayerAdrenaline] PlayerMovement or its Rigidbody is unavailable. Continuous speed gain is disabled.", this);
        }

        private void OnValidate()
        {
            _minimumSpeedForGain = Mathf.Max(0f, _minimumSpeedForGain);
            _speedForMaximumGain = Mathf.Max(_minimumSpeedForGain + MinimumSpeedSeparation, _speedForMaximumGain);
            _maximumSpeedFillPerSecond = Mathf.Max(0f, _maximumSpeedFillPerSecond);
            _killFillAmount = Mathf.Max(0f, _killFillAmount);
            _damageDrainPerDamage = Mathf.Max(0f, _damageDrainPerDamage);
            _disengagementThreshold = Mathf.Max(0f, _disengagementThreshold);
            _passiveDrainPerSecond = Mathf.Max(0f, _passiveDrainPerSecond);

            if (_speedFillCurve == null || _speedFillCurve.length == 0)
            {
                _speedFillCurve = CreateDefaultSpeedFillCurve();
            }

            if (_damageReductionPerTier == null || _damageReductionPerTier.Length == 0)
            {
                Debug.LogWarning("[PlayerAdrenaline] Damage reduction tiers are missing. Adrenaline will provide no damage reduction.", this);
                return;
            }

            _damageReductionPerTier[0] = 0f;
            for (int tierIndex = 1; tierIndex < _damageReductionPerTier.Length; tierIndex++)
            {
                _damageReductionPerTier[tierIndex] = Mathf.Clamp01(_damageReductionPerTier[tierIndex]);
            }
        }

        private static AnimationCurve CreateDefaultSpeedFillCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 0f),
                new Keyframe(0.5f, 0.08f),
                new Keyframe(0.75f, 0.42f),
                new Keyframe(0.9f, 0.68f),
                new Keyframe(1f, 0.78f)
            );
        }
    }
}
