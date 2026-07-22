using Combat.VFX;
using Health;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(AIBrain))]
[RequireComponent(typeof(AIAiming))]
[RequireComponent(typeof(AIFiring))]
public sealed class AIDeath : MonoBehaviour
{
    private const float MinimumVfxDimension = 0.01f;

    [Header("References")]
    [SerializeField]
    private AudioPool audioPool;

    [Header("Death VFX")]
    [Tooltip("Optional shared pool used to spawn the death explosion.")]
    [SerializeField]
    private DeathVfxPool _deathVfxPool;

    [SerializeField]
    private Sound _deathSound;

    [Tooltip("World-aligned width, height, and depth applied to particle emission shapes.")]
    [SerializeField]
    private Vector3 _vfxDimensions = Vector3.one;

    private EnemyHealth _enemyHealth;
    private AIBrain _brain;
    private AIAiming _aiming;
    private AIFiring _firing;
    private bool _hasHandledDeath;

    private void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
        _brain = GetComponent<AIBrain>();
        _aiming = GetComponent<AIAiming>();
        _firing = GetComponent<AIFiring>();
        if (audioPool == null)
        {
            Debug.LogError("[AIDeath] has no ref to audio pool");
        }
        _enemyHealth.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnDeath -= HandleDeath;
    }

    private void OnValidate()
    {
        _vfxDimensions.x = Mathf.Max(MinimumVfxDimension, _vfxDimensions.x);
        _vfxDimensions.y = Mathf.Max(MinimumVfxDimension, _vfxDimensions.y);
        _vfxDimensions.z = Mathf.Max(MinimumVfxDimension, _vfxDimensions.z);
    }

    private void HandleDeath()
    {
        if (_hasHandledDeath)
            return;

        _hasHandledDeath = true;
        _brain.enabled = false;
        _aiming.enabled = false;
        _firing.enabled = false;

        if (_deathVfxPool == null)
            return;

        _deathVfxPool.Spawn(transform.position, transform.rotation, _vfxDimensions);
        audioPool.PlayAt(_deathSound, transform.position);
        gameObject.SetActive(false);
    }
}
