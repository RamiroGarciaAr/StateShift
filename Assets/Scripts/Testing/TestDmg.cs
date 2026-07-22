using Health;
using UnityEngine;

public class TestDmg : MonoBehaviour
{
    private enum IncomingDirectionMode
    {
        Fallback,
        ConfiguredDirection
    }

    [Header("Damage")]
    [Tooltip("Base damage applied when the temporary damage trigger is pressed.")]
    [SerializeField, Range(1f, 100f)] private float _damageAmount = 25f;

    [Tooltip("Controls whether the test hit supplies an incoming projectile direction or exercises the top-center fallback.")]
    [SerializeField] private IncomingDirectionMode _incomingDirectionMode = IncomingDirectionMode.Fallback;

    [Tooltip("Representative world-space projectile travel direction used when Configured Direction is selected.")]
    [SerializeField] private Vector3 _incomingDirection = Vector3.forward;

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponentInChildren<PlayerHealth>();
        if (_playerHealth == null)
            Debug.LogWarning("[TestDmg] No PlayerHealth found in children.", this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && _playerHealth != null)
            ApplyTestDamage();
    }

    private void ApplyTestDamage()
    {
        Vector3 hitDirection = _incomingDirectionMode == IncomingDirectionMode.ConfiguredDirection
            ? _incomingDirection
            : Vector3.zero;

        DamageInfo damageInfo = new DamageInfo(
            baseDamage: _damageAmount,
            damageType: DamageType.Kinetic,
            instigator: Instigator.Enemy,
            hitDirection: hitDirection
        );
        _playerHealth.TakeDamage(damageInfo);
    }
}
