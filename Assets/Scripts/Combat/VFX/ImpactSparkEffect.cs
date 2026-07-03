using System;
using UnityEngine;

namespace Combat.VFX
{
    /// <summary>
    /// Drives the spark burst, flash particle systems, and the white flash light on the spark prefab.
    /// Raises <see cref="Finished"/> when the effect completes so the owning pool can release it.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class ImpactSparkEffect : MonoBehaviour
    {
        [Header("Flash")]
        [Tooltip("Point light that pulses white on impact. Driven entirely by this component.")]
        [SerializeField] private Light _flashLight;

        [Tooltip("Peak intensity the flash light is set to when the effect plays.")]
        [SerializeField] private float _flashIntensity = 6f;

        [Tooltip("Seconds over which the flash light fades from peak intensity to zero.")]
        [SerializeField] private float _flashDurationSeconds = 0.08f;

        private ParticleSystem _controlSystem;
        private float _flashElapsed;
        private bool _isPlaying;

        /// <summary>
        /// Raised once the control particle system is no longer alive.
        /// </summary>
        public event Action<ImpactSparkEffect> Finished;

        private void Awake()
        {
            _controlSystem = GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// Restarts the spark burst, flash, and light pulse from the current transform.
        /// </summary>
        public void Play()
        {
            gameObject.SetActive(true);
            _flashElapsed = 0f;
            _isPlaying = true;

            if (_flashLight != null)
            {
                _flashLight.intensity = _flashIntensity;
                _flashLight.enabled = true;
            }

            _controlSystem.Clear(true);
            _controlSystem.Play(true);
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            if (_flashLight != null && _flashLight.enabled)
            {
                _flashElapsed += Time.deltaTime;
                float t = _flashDurationSeconds > 0f ? Mathf.Clamp01(_flashElapsed / _flashDurationSeconds) : 1f;
                _flashLight.intensity = Mathf.Lerp(_flashIntensity, 0f, t);
                if (t >= 1f)
                    _flashLight.enabled = false;
            }

            if (!_controlSystem.IsAlive(true))
            {
                _isPlaying = false;
                if (_flashLight != null)
                    _flashLight.enabled = false;

                Finished?.Invoke(this);
                gameObject.SetActive(false);
            }
        }
    }
}
