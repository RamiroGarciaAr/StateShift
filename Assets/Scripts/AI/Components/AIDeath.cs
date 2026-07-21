using Combat.VFX;
using Health;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(AIBrain))]
[RequireComponent(typeof(AIAiming))]
[RequireComponent(typeof(AIFiring))]
public sealed class AIDeath : MonoBehaviour
{
    private const float MinimumVfxDimension = 0.01f;

    [Header("Death VFX")]
    [Tooltip("Optional shared pool used to spawn the death explosion.")]
    [SerializeField]
    private DeathVfxPool _deathVfxPool;

    [Tooltip("World-aligned width, height, and depth applied to particle emission shapes.")]
    [SerializeField]
    private Vector3 _vfxDimensions = Vector3.one;

    [Header("Renderers")]
    [Tooltip("Visible enemy renderers disabled immediately when death occurs.")]
    [SerializeField]
    private Renderer[] _renderersToHide;

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

        if (_renderersToHide != null)
        {
            for (int index = 0; index < _renderersToHide.Length; index++)
            {
                Renderer rendererToHide = _renderersToHide[index];
                if (rendererToHide != null)
                    rendererToHide.enabled = false;
            }
        }

        if (_deathVfxPool == null)
            return;

        _deathVfxPool.Spawn(transform.position, transform.rotation, _vfxDimensions);
    }
}
