using System;
using Combat.VFX;
using Health;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [Header("Collision")]
    [Tooltip("Layers this projectile can hit.")]
    [SerializeField] private LayerMask _hitMask = ~0;

    private WeaponDataSO _data;
    private ImpactEffectSpawner _impactSpawner;
    private Transform _cachedTransform;
    private TrailRenderer _trailRenderer;
    private Vector3 _spawnPos;
    private Vector3 _currentVelocity;
    private Action<ProjectileBase> _releaseAction;
    private bool _hasHit;
    private float _timeAlive;
    private Instigator _instigator;

    private void Awake()
    {
        _cachedTransform = transform;
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Resets transform and pooled visual state before this projectile is configured for a new shot.
    /// </summary>
    public void PrepareForReuse(Vector3 position, Quaternion rotation, Action<ProjectileBase> releaseAction)
    {
        _cachedTransform.SetPositionAndRotation(position, rotation);
        _releaseAction = releaseAction;
        _trailRenderer?.Clear();
    }

    /// <summary>
    /// Configures the projectile with its weapon data and the injected impact effect spawner.
    /// </summary>
    public void Initialize(
        WeaponDataSO data,
        ImpactEffectSpawner impactSpawner,
        Instigator instigator
    )
    {
        _data = data;
        _impactSpawner = impactSpawner;
        _spawnPos = _cachedTransform.position;
        _currentVelocity = _cachedTransform.forward * _data.ProjectileSpeed;
        _hasHit = false;
        _timeAlive = 0f;
        _instigator = instigator;
    }

    private void Update()
    {
        if (_hasHit || _data == null)
            return;

        _timeAlive += Time.deltaTime;
        if (_timeAlive >= _data.ProjectileLifetime)
        {
            DeactivateProjectile();
            return;
        }

        Vector3 frameTranslation = _currentVelocity * Time.deltaTime;
        if (Physics.Raycast(
            _cachedTransform.position,
            _currentVelocity.normalized,
            out RaycastHit hit,
            frameTranslation.magnitude,
            _hitMask,
            QueryTriggerInteraction.Ignore))
        {
            _hasHit = true;
            _cachedTransform.position = hit.point;

            bool spawnDecal = true;
            if (TryGetHitbox(hit.collider, out Hitbox hitbox))
            {
                TryDealDamage(hitbox, hit);
                spawnDecal = false;
            }
            else if (hit.collider.GetComponentInParent<IDamageable>() != null)
            {
                spawnDecal = false;
            }

            _impactSpawner?.SpawnImpact(hit.point, hit.normal, spawnDecal);
            DeactivateProjectile();
            return;
        }

        _cachedTransform.position += frameTranslation;
    }

    private void TryDealDamage(Hitbox hitbox, RaycastHit hit)
    {
        if (hitbox.Damageable == null || !hitbox.Damageable.IsAlive)
            return;

        float distance = Vector3.Distance(_spawnPos, hit.point);
        float multiplier = _data.GetDamageMultiplierAtDistance(distance);
        float finalDamage = _data.DamageAmount * multiplier;
        Vector3 hitDirection = _currentVelocity.sqrMagnitude > Mathf.Epsilon
            ? _currentVelocity.normalized
            : Vector3.zero;
        DamageInfo damageInfo = new DamageInfo(
            baseDamage: finalDamage,
            damageType: _data.DamageType,
            bodyPart: hitbox.BodyPart,
            instigator: _instigator,
            hitPoint: hit.point,
            hitDirection: hitDirection
        );
        hitbox.Damageable.TakeDamage(damageInfo);
    }

    private static bool TryGetHitbox(Collider hitCollider, out Hitbox hitbox)
    {
        hitbox = null;
        if (hitCollider == null)
            return false;

        if (hitCollider.TryGetComponent(out hitbox))
            return true;

        Transform parentTransform = hitCollider.transform.parent;
        if (parentTransform == null)
            return false;

        hitbox = parentTransform.GetComponentInParent<Hitbox>();
        return hitbox != null;
    }

    private void DeactivateProjectile()
    {
        if (_releaseAction != null)
        {
            _releaseAction.Invoke(this);
            return;
        }

        gameObject.SetActive(false);
    }
}
