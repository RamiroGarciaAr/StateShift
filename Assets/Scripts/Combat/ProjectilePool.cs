using System;
using Combat.VFX;
using Health;
using UnityEngine;
using UnityEngine.Pool;

public sealed class ProjectilePool : MonoBehaviour
{
    [Header("Projectile Pool")]
    [Tooltip("Projectile prefab that this pool creates and reuses.")]
    [SerializeField] private ProjectileBase _projectilePrefab;

    [Tooltip("Initial internal storage capacity for pooled projectiles.")]
    [SerializeField, Min(0)] private int _defaultCapacity = 16;

    [Tooltip("Maximum number of inactive projectiles retained by the pool.")]
    [SerializeField, Min(1)] private int _maxPoolSize = 64;

    private ObjectPool<ProjectileBase> _pool;

    private void Awake()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogError($"[ProjectilePool] Projectile prefab is not assigned on {name}.", this);
            enabled = false;
            return;
        }

        _pool = new ObjectPool<ProjectileBase>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            false,
            _defaultCapacity,
            _maxPoolSize
        );
    }

    /// <summary>
    /// Retrieves, configures, and activates a projectile from this pool.
    /// </summary>
    public ProjectileBase Spawn(
        Vector3 position,
        Quaternion rotation,
        WeaponDataSO weaponData,
        ImpactEffectSpawner impactSpawner,
        Instigator instigator
    )
    {
        if (_pool == null)
        {
            Debug.LogError($"[ProjectilePool] Pool is unavailable on {name}.", this);
            return null;
        }

        ProjectileBase projectile = _pool.Get();
        projectile.PrepareForReuse(position, rotation, ReleaseProjectile);
        projectile.Initialize(weaponData, impactSpawner, instigator);
        return projectile;
    }

    private ProjectileBase CreateProjectile()
    {
        ProjectileBase projectile = Instantiate(_projectilePrefab, transform);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private static void OnGetProjectile(ProjectileBase projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private static void OnReleaseProjectile(ProjectileBase projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    private static void OnDestroyProjectile(ProjectileBase projectile)
    {
        Destroy(projectile.gameObject);
    }

    private void ReleaseProjectile(ProjectileBase projectile)
    {
        _pool.Release(projectile);
    }
}
