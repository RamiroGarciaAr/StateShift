using Health;
using UnityEngine;

// Manual simulation — NO Rigidbody needed. Remove it from the prefab.
public class ProjectileBase : MonoBehaviour
{
    private WeaponDataSO _data;
    private Vector3      _spawnPos;
    private Vector3      _currentVelocity;
    private bool         _hasHit;
    private float        _timeAlive;

    [SerializeField] private LayerMask _hitMask = ~0;

    public void Initialise(WeaponDataSO data)
    {
        _data            = data;
        _spawnPos        = transform.position;
        _currentVelocity = transform.forward * _data.ProjectileSpeed;
        _hasHit          = false;
        _timeAlive       = 0f;
    }

    private void Update()
    {
        if (_hasHit || _data == null) return;

        _timeAlive += Time.deltaTime;
        if (_timeAlive >= _data.ProjectileLifetime)
        {
            DeactivateProjectile();
            return;
        }

        Vector3 frameTranslation = _currentVelocity * Time.deltaTime;

        if (Physics.Raycast(
                transform.position,
                _currentVelocity.normalized,
                out RaycastHit hit,
                frameTranslation.magnitude,
                _hitMask,
                QueryTriggerInteraction.Ignore))
        {
            _hasHit            = true;
            transform.position = hit.point;
            TryDealDamage(hit);
            DeactivateProjectile();
        }
        else
        {
            transform.position += frameTranslation;
        }
    }

    private void TryDealDamage(RaycastHit hit)
    {
        IDamagable target = hit.collider.GetComponentInParent<IDamagable>();
        if (target == null || !target.IsAlive) return;

        float distance    = Vector3.Distance(_spawnPos, hit.point);
        float multiplier  = _data.GetDamageMultiplierAtDistance(distance);
        float finalDamage = _data.DamageAmount * multiplier;

        // TODO: Use hit.normal for decals, VFX, hit direction effects
        var damageInfo = new DamageInfo(
            baseDamage: finalDamage,
            damageType: _data.DamageType,
            hitPoint:   hit.point
        );

        target.TakeDamage(damageInfo);
    }

    private void DeactivateProjectile()
    {
        gameObject.SetActive(false);
    }
}