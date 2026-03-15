using Health;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileBase : MonoBehaviour
{
    private WeaponDataSO _data;
    private Vector3 _spawnPos;
    private Rigidbody _rb;
    private bool _hasHit;

    public void Initialise(WeaponDataSO data)
    {
        _data = data;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;

        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        _spawnPos = transform.position;

        if (_data == null)
        {
            Debug.LogError("[ProjectileBase] Projectile has no data assigned");

            Destroy(gameObject);
            return;
        }

        _rb.velocity = transform.forward * _data.ProjectileSpeed;
        Destroy(gameObject, _data.ProjectileLifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;

        _hasHit = true;
        
        TryDealDamage(collision);
        Destroy(gameObject); // We destroy the projectile on hit regardless of whether it hit a damagable target or not, to avoid it bouncing around and hitting multiple targets.
    }

    private void TryDealDamage(Collision collision)
    {
        IDamagable target = collision.gameObject.GetComponentInParent<IDamagable>();        if (target == null) return;

        if (!target.IsAlive) return;

        float distance = Vector3.Distance(_spawnPos, transform.position);
        float multiplier = _data.GetDamageMultiplierAtDistance(distance);
        float damageAmount = _data.DamageAmount * multiplier;

        //TODO: Add Contact Point Info to DamageInfo and use that for hit effects, decals, etc.

        var damageInfo = new DamageInfo
        (
            baseDamage: damageAmount,
            damageType: _data.DamageType,
            hitPoint: collision.GetContact(0).point
        );

        target.TakeDamage(damageInfo);
    }

}
