using Combat.VFX;
using Health;
using UnityEngine;

// Manual simulation — NO Rigidbody needed.
public class ProjectileBase : MonoBehaviour
{
    private WeaponDataSO _data;
    private ImpactEffectSpawner _impactSpawner;
    private Vector3 _spawnPos;
    private Vector3 _currentVelocity;
    private bool _hasHit;
    private float _timeAlive;

    [SerializeField]
    private LayerMask _hitMask = ~0;

    private Instigator _instigator;

    /// <summary>
    /// Configures the projectile with its weapon data and the injected impact effect spawner.
    /// </summary>
    /// <param name="data">Weapon data driving speed, lifetime, and damage.</param>
    /// <param name="impactSpawner">Pooled spawner used to play impact VFX on every hit.</param>
    public void Initialize(
        WeaponDataSO data,
        ImpactEffectSpawner impactSpawner,
        Instigator instigator
    )
    {
        _data = data;
        _impactSpawner = impactSpawner;
        _spawnPos = transform.position;
        _currentVelocity = transform.forward * _data.ProjectileSpeed;
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

        if (
            Physics.Raycast(
                transform.position,
                _currentVelocity.normalized,
                out RaycastHit hit,
                frameTranslation.magnitude,
                _hitMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            _hasHit = true;
            transform.position = hit.point;

            // Resolve the damageable once: gate the decal on it and reuse it for damage dealing.
            // GetComponentInParent so colliders on child bones/limbs still count as damageable.
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            _impactSpawner?.SpawnImpact(hit.point, hit.normal, spawnDecal: target == null);
            TryDealDamage(target, hit);
            DeactivateProjectile();
        }
        else
        {
            transform.position += frameTranslation;
        }
    }

    private void TryDealDamage(IDamageable target, RaycastHit hit)
    {
        if (target == null || !target.IsAlive)
            return;

        float distance = Vector3.Distance(_spawnPos, hit.point);
        float multiplier = _data.GetDamageMultiplierAtDistance(distance);
        float finalDamage = _data.DamageAmount * multiplier;

        var damageInfo = new DamageInfo(
            baseDamage: finalDamage,
            damageType: _data.DamageType,
            instigator: _instigator,
            hitPoint: hit.point
        );

        target.TakeDamage(damageInfo);
        Debug.Log($"instigator: {_instigator}");
    }

    private void DeactivateProjectile()
    {
        gameObject.SetActive(false);
    }
}
