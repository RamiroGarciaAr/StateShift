using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Combat.VFX
{
    /// <summary>
    /// Scene-placed owner of the spark and scorch-decal pools. Dependency-injected into
    /// projectiles via their Initialise call so no singleton or GameObject.Find is required.
    /// Generates the shared runtime textures once and caps the number of active decals.
    /// </summary>
    public sealed class ImpactEffectSpawner : MonoBehaviour
    {
        private const int SoftDotTextureSize = 64;
        private const int ScorchTextureSize = 128;
        private const float DecalNormalOffset = 0.01f;
        private const float VerticalNormalThreshold = 0.99f;

        [Header("Prefabs")]
        [Tooltip("Spark burst + flash effect spawned at every impact.")]
        [SerializeField]
        private ImpactSparkEffect _sparkEffectPrefab;

        [Tooltip("Scorch decal quad spawned on the impacted surface.")]
        [SerializeField]
        private ScorchDecal _scorchDecalPrefab;

        [Header("Pooling")]
        [Tooltip("Initial capacity allocated for each effect pool.")]
        [SerializeField]
        private int _defaultPoolSize = 16;

        [Tooltip("Maximum retained instances per pool before extras are destroyed on release.")]
        [SerializeField]
        private int _maxPoolSize = 64;

        [Header("Decal")]
        [Tooltip("Maximum number of scorch decals allowed to persist at once.")]
        [SerializeField]
        private int _maxActiveDecals = 48;

        [Tooltip("Random lifetime range (min, max) in seconds for each scorch decal.")]
        [SerializeField]
        private Vector2 _decalLifetimeSeconds = new Vector2(10f, 15f);

        [Tooltip("Uniform world-space size of each scorch decal quad.")]
        [SerializeField]
        private float _decalSize = 0.35f;

        [Header("Materials")]
        [Tooltip(
            "Shared additive material for the spark and flash particles. Texture generated at runtime."
        )]
        [SerializeField]
        private Material _sparkMaterial;

        [Tooltip("Shared decal material. Texture generated at runtime.")]
        [SerializeField]
        private Material _scorchMaterial;
        private ObjectPool<ImpactSparkEffect> _sparkPool;
        private ObjectPool<ScorchDecal> _decalPool;
        private readonly Queue<ScorchDecal> _activeDecals = new Queue<ScorchDecal>();
        private bool _isInitialised;

        private void Awake()
        {
            if (
                _sparkEffectPrefab == null
                || _scorchDecalPrefab == null
                || _sparkMaterial == null
                || _scorchMaterial == null
            )
            {
                Debug.LogError(
                    $"{nameof(ImpactEffectSpawner)} is missing prefab or material references. Disabling.",
                    this
                );
                enabled = false;
                return;
            }

            _sparkMaterial.mainTexture = ProceduralTextures.CreateHexagon(SoftDotTextureSize);
            _scorchMaterial.mainTexture = ProceduralTextures.CreateScorch(ScorchTextureSize);

            _sparkPool = new ObjectPool<ImpactSparkEffect>(
                CreateSpark,
                OnGetSpark,
                OnReleaseSpark,
                OnDestroySpark,
                collectionCheck: true,
                defaultCapacity: _defaultPoolSize,
                maxSize: _maxPoolSize
            );

            _decalPool = new ObjectPool<ScorchDecal>(
                CreateDecal,
                OnGetDecal,
                OnReleaseDecal,
                OnDestroyDecal,
                collectionCheck: true,
                defaultCapacity: _defaultPoolSize,
                maxSize: _maxPoolSize
            );

            _isInitialised = true;
        }

        /// <summary>
        /// Spawns a spark burst oriented to the surface normal at the impact point. The scorch decal
        /// is only spawned when <paramref name="spawnDecal"/> is true, so callers can suppress the
        /// persistent decal on moving targets (e.g. enemies) while still playing the spark.
        /// </summary>
        /// <param name="point">World-space impact position.</param>
        /// <param name="normal">Surface normal at the impact point.</param>
        /// <param name="spawnDecal">When true, a scorch decal is placed on the impacted surface.</param>
        public void SpawnImpact(Vector3 point, Vector3 normal, bool spawnDecal = true)
        {
            if (!_isInitialised)
                return;

            Quaternion orientation = BuildSurfaceOrientation(normal);

            ImpactSparkEffect spark = _sparkPool.Get();
            spark.transform.SetPositionAndRotation(point, orientation);
            spark.Play();

            if (spawnDecal)
            {
                if (_activeDecals.Count >= _maxActiveDecals)
                {
                    ScorchDecal oldest = _activeDecals.Dequeue();
                    if (oldest != null)
                        oldest.ForceRelease();
                }

                ScorchDecal decal = _decalPool.Get();
                decal.transform.SetPositionAndRotation(
                    point + normal * DecalNormalOffset,
                    orientation
                );
                decal.transform.localScale = new Vector3(_decalSize, _decalSize, _decalSize);
                float lifetime = Random.Range(_decalLifetimeSeconds.x, _decalLifetimeSeconds.y);
                decal.Play(lifetime);
                _activeDecals.Enqueue(decal);
            }
        }

        /// <summary>
        /// Builds a rotation whose forward axis points along the surface normal, so the spark cone
        /// fires away from the surface and the decal quad lies flat on it. Uses a stable up vector
        /// to avoid the degenerate LookRotation result when the normal is near-vertical.
        /// </summary>
        private static Quaternion BuildSurfaceOrientation(Vector3 normal)
        {
            if (normal.sqrMagnitude < Mathf.Epsilon)
                return Quaternion.identity;

            Vector3 up =
                Mathf.Abs(Vector3.Dot(normal.normalized, Vector3.up)) > VerticalNormalThreshold
                    ? Vector3.forward
                    : Vector3.up;

            return Quaternion.LookRotation(normal, up);
        }

        private ImpactSparkEffect CreateSpark()
        {
            ImpactSparkEffect instance = Instantiate(_sparkEffectPrefab, transform);
            instance.Finished += OnSparkFinished;
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnGetSpark(ImpactSparkEffect spark)
        {
            spark.gameObject.SetActive(true);
        }

        private void OnReleaseSpark(ImpactSparkEffect spark)
        {
            spark.gameObject.SetActive(false);
        }

        private void OnDestroySpark(ImpactSparkEffect spark)
        {
            if (spark == null)
                return;

            spark.Finished -= OnSparkFinished;
            Destroy(spark.gameObject);
        }

        private void OnSparkFinished(ImpactSparkEffect spark)
        {
            _sparkPool.Release(spark);
        }

        private ScorchDecal CreateDecal()
        {
            ScorchDecal instance = Instantiate(_scorchDecalPrefab, transform);
            instance.Finished += OnDecalFinished;
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnGetDecal(ScorchDecal decal)
        {
            decal.gameObject.SetActive(true);
        }

        private void OnReleaseDecal(ScorchDecal decal)
        {
            decal.gameObject.SetActive(false);
        }

        private void OnDestroyDecal(ScorchDecal decal)
        {
            if (decal == null)
                return;

            decal.Finished -= OnDecalFinished;
            Destroy(decal.gameObject);
        }

        private void OnDecalFinished(ScorchDecal decal)
        {
            _decalPool.Release(decal);
        }
    }
}
