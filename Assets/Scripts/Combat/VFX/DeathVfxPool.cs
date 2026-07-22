using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Combat.VFX
{
    /// <summary>
    /// Owns a bounded pool of reusable one-shot death particle effects.
    /// </summary>
    public sealed class DeathVfxPool : MonoBehaviour
    {
        private const int MinimumPoolCapacity = 1;
        private const int DefaultPoolCapacity = 4;
        private const int MaximumPoolCapacity = 16;
        private const float MinimumDimension = 0.01f;

        [Header("Prefab")]
        [Tooltip("One-shot particle hierarchy whose root contains the lifetime control ParticleSystem.")]
        [SerializeField]
        private GameObject _particlePrefab;

        [Header("Pooling")]
        [Tooltip("Initial retained capacity for death effects.")]
        [SerializeField]
        private int _defaultPoolSize = DefaultPoolCapacity;

        [Tooltip("Maximum number of retained death effects.")]
        [SerializeField]
        private int _maxPoolSize = MaximumPoolCapacity;

        private ObjectPool<PooledDeathVfx> _pool;
        private List<PooledDeathVfx> _activeEffects;
        private bool _isInitialised;

        private void Awake()
        {
            OnValidate();

            if (_particlePrefab == null || _particlePrefab.GetComponent<ParticleSystem>() == null)
            {
                Debug.LogError(
                    $"{nameof(DeathVfxPool)} on {gameObject.name} requires a particle prefab with a root {nameof(ParticleSystem)}. Disabling.",
                    this
                );
                enabled = false;
                return;
            }

            _activeEffects = new List<PooledDeathVfx>(_maxPoolSize);
            _pool = new ObjectPool<PooledDeathVfx>(
                CreateEffect,
                OnGetEffect,
                OnReleaseEffect,
                OnDestroyEffect,
                collectionCheck: true,
                defaultCapacity: _defaultPoolSize,
                maxSize: _maxPoolSize
            );
            _isInitialised = true;
        }

        private void Update()
        {
            for (int index = _activeEffects.Count - 1; index >= 0; index--)
            {
                PooledDeathVfx effect = _activeEffects[index];
                if (effect.RootSystem.IsAlive(true))
                    continue;

                _activeEffects.RemoveAt(index);
                _pool.Release(effect);
            }
        }

        private void OnValidate()
        {
            _defaultPoolSize = Mathf.Max(MinimumPoolCapacity, _defaultPoolSize);
            _maxPoolSize = Mathf.Max(_defaultPoolSize, _maxPoolSize);
        }

        /// <summary>
        /// Plays a retained death effect at the requested world pose and applies independent shape dimensions.
        /// </summary>
        /// <param name="position">World-space position for the effect root.</param>
        /// <param name="rotation">World-space rotation for the effect root.</param>
        /// <param name="dimensions">Width, height, and depth multipliers for enabled particle shapes.</param>
        public void Spawn(Vector3 position, Quaternion rotation, Vector3 dimensions)
        {
            if (!_isInitialised)
                return;

            dimensions.x = Mathf.Max(MinimumDimension, dimensions.x);
            dimensions.y = Mathf.Max(MinimumDimension, dimensions.y);
            dimensions.z = Mathf.Max(MinimumDimension, dimensions.z);

            PooledDeathVfx effect = _pool.Get();
            Transform effectTransform = effect.Transform;
            effectTransform.SetPositionAndRotation(position, rotation);
            effectTransform.localScale = Vector3.one;
            effect.ApplyDimensions(dimensions);
            effect.RootSystem.Clear(true);
            effect.RootSystem.Play(true);
            _activeEffects.Add(effect);
        }

        private PooledDeathVfx CreateEffect()
        {
            GameObject instance = Instantiate(_particlePrefab, transform);
            ParticleSystem rootSystem = instance.GetComponent<ParticleSystem>();
            ParticleSystem[] allSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            int shapedSystemCount = 0;

            for (int index = 0; index < allSystems.Length; index++)
            {
                if (allSystems[index].shape.enabled)
                    shapedSystemCount++;
            }

            ParticleSystem[] shapedSystems = new ParticleSystem[shapedSystemCount];
            Vector3[] baselineShapeScales = new Vector3[shapedSystemCount];
            int shapedIndex = 0;

            for (int index = 0; index < allSystems.Length; index++)
            {
                ParticleSystem.ShapeModule shape = allSystems[index].shape;
                if (!shape.enabled)
                    continue;

                shapedSystems[shapedIndex] = allSystems[index];
                baselineShapeScales[shapedIndex] = shape.scale;
                shapedIndex++;
            }

            instance.SetActive(false);
            return new PooledDeathVfx(instance.transform, rootSystem, shapedSystems, baselineShapeScales);
        }

        private static void OnGetEffect(PooledDeathVfx effect)
        {
            effect.Transform.gameObject.SetActive(true);
        }

        private static void OnReleaseEffect(PooledDeathVfx effect)
        {
            effect.RootSystem.Clear(true);
            effect.RestoreShapeScales();
            effect.Transform.localScale = Vector3.one;
            effect.Transform.gameObject.SetActive(false);
        }

        private static void OnDestroyEffect(PooledDeathVfx effect)
        {
            if (effect != null && effect.Transform != null)
                Destroy(effect.Transform.gameObject);
        }

        private sealed class PooledDeathVfx
        {
            private readonly ParticleSystem[] _shapedSystems;
            private readonly Vector3[] _baselineShapeScales;

            public PooledDeathVfx(
                Transform transform,
                ParticleSystem rootSystem,
                ParticleSystem[] shapedSystems,
                Vector3[] baselineShapeScales
            )
            {
                Transform = transform;
                RootSystem = rootSystem;
                _shapedSystems = shapedSystems;
                _baselineShapeScales = baselineShapeScales;
            }

            public Transform Transform { get; }
            public ParticleSystem RootSystem { get; }

            public void ApplyDimensions(Vector3 dimensions)
            {
                for (int index = 0; index < _shapedSystems.Length; index++)
                {
                    ParticleSystem.ShapeModule shape = _shapedSystems[index].shape;
                    shape.scale = Vector3.Scale(_baselineShapeScales[index], dimensions);
                }
            }

            public void RestoreShapeScales()
            {
                for (int index = 0; index < _shapedSystems.Length; index++)
                {
                    ParticleSystem.ShapeModule shape = _shapedSystems[index].shape;
                    shape.scale = _baselineShapeScales[index];
                }
            }
        }
    }
}
