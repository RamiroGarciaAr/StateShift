using Combat.VFX;
using Health;
using UnityEngine;

public class AIFiring : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private AudioPool audioPool;

    [Header("Firing")]
    [Tooltip("Muzzle transforms used in sequence when firing.")]
    [SerializeField]
    private Transform[] _firePoints;

    [Tooltip("Transform whose forward direction is compared against the target.")]
    [SerializeField]
    private Transform _aimReference;

    [Tooltip(
        "Maximum angle in degrees between the aim direction and target direction before firing."
    )]
    [SerializeField, Range(0f, 90f)]
    private float _fireCone = 10f;

    [Tooltip("Minimum time in seconds between shots.")]
    [SerializeField, Min(0f)]
    private float _fireInterval = 0.6f;

    [Header("Projectile")]
    [Tooltip("Pool that provides this enemy's projectiles.")]
    [SerializeField]
    private ProjectilePool _projectilePool;

    [Tooltip("Weapon data used to configure projectile speed, lifetime, and damage.")]
    [SerializeField]
    private WeaponDataSO _weaponData;

    [Tooltip("Optional pooled impact effect spawner passed to each projectile.")]
    [SerializeField]
    private ImpactEffectSpawner _impactSpawner;

    [Header("Accuracy")]
    [SerializeField, Tooltip("Time that the AI reacts and stats to shoot")]
    float newTargetDelay = 0.3f;

    [SerializeField, Tooltip("Max spread in degrees applied per shot")]
    private float maxSpread = 6f;

    [SerializeField, Tooltip("Min spread in degrees applied per shot")]
    private float minSpread = 1f;

    [SerializeField, Tooltip("Time of Continued LOS to go from inaccurate to accurate")]
    private float rampUpTime = 2f;

    [Header("AI Sounds")]
    [SerializeField]
    private Sound detectedSound;
    private bool _canSee;
    private Vector3 _target;
    private float _rateOfFireTimer;
    private float _reactionTimer; // Counts the time for the AI's Reaction
    private float _lineOfSightTimer; // The timer for unbroken LOS

    private int _nextBarrel;

    private void Awake()
    {
        if (_firePoints == null || _firePoints.Length == 0)
        {
            Debug.LogError($"[AIFiring] Fire points are not configured on {name}.", this);
            enabled = false;
            return;
        }

        if (_aimReference == null || _projectilePool == null || _weaponData == null)
        {
            Debug.LogError(
                $"[AIFiring] Aim reference, projectile pool, and weapon data must be configured on {name}.",
                this
            );
            enabled = false;
        }
    }

    /// <summary>
    /// Updates the AI's current line-of-sight and target position for firing decisions.
    /// </summary>
    public void SetFiringState(bool canSee, Vector3 point)
    {
        if (canSee && !_canSee) // we are seeing him for the first time
        {
            audioPool.PlayAt(detectedSound, transform.position);
            _reactionTimer = newTargetDelay;
            _lineOfSightTimer = 0f;
        }

        _canSee = canSee;
        _target = point;
    }

    private void Update()
    {
        _rateOfFireTimer -= Time.deltaTime;
        if (!_canSee)
        {
            _lineOfSightTimer = 0f;
            return;
        }
        _lineOfSightTimer += Time.deltaTime;
        _reactionTimer -= Time.deltaTime;

        if (!CanFire())
            return;

        Fire();
        _rateOfFireTimer = _fireInterval;
    }

    private bool CanFire()
    {
        if (_rateOfFireTimer > 0f || _reactionTimer > 0f)
            return false;

        Vector3 toTarget = _target - _aimReference.position;
        return Vector3.Angle(_aimReference.forward, toTarget) <= _fireCone;
    }

    private void Fire()
    {
        Transform barrelTransform = _firePoints[_nextBarrel];
        _nextBarrel = (_nextBarrel + 1) % _firePoints.Length;

        Vector3 dir = _target - barrelTransform.position;

        float t = Mathf.Clamp01(_lineOfSightTimer / rampUpTime);
        float spread = Mathf.Lerp(maxSpread, minSpread, t);
        dir =
            Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f)
            * dir;
        audioPool.PlayAt(_weaponData.FireSounds, barrelTransform.position);
        _projectilePool.Spawn(
            barrelTransform.position,
            Quaternion.LookRotation(dir),
            _weaponData,
            _impactSpawner,
            Instigator.Enemy
        );
    }
}
