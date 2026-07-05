using System.Collections;
using Health;
using UnityEngine;

// Temporary test component
public class DamageTestTarget : MonoBehaviour, IDamageable
{
    private const string IdleStateName = "Idle";
    private const string HasHitTrigger = "HasHit";
    private const string HasDiedTrigger = "HasDied";

    [Header("Health")]
    [Tooltip("Starting health restored when the target resets.")]
    [SerializeField]
    private float _maxHealth = 100f;

    [Header("Reset")]
    [Tooltip("Seconds the target stays down before resetting to Idle.")]
    [SerializeField]
    private float _resetDelaySeconds = 10f;

    private Animator _animator;
    private WaitForSeconds _resetWait;
    private Coroutine _resetRoutine;
    private float _health;

    public bool IsAlive => _health > 0f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _resetWait = new WaitForSeconds(_resetDelaySeconds);
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!IsAlive)
        {
            return;
        }

        _animator.SetTrigger(HasHitTrigger);
        _health -= damageInfo.FinalDamage;

        if (!IsAlive)
        {
            _animator.SetTrigger(HasDiedTrigger);
            _resetRoutine = StartCoroutine(ResetAfterDelay());
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return _resetWait;
        ResetTarget();
    }

    private void ResetTarget()
    {
        Debug.Log("Resetting target to Idle state.");
        _health = _maxHealth;
        _animator.ResetTrigger(HasHitTrigger);
        _animator.ResetTrigger(HasDiedTrigger);
        _animator.Play(IdleStateName, 0, 0f);
        _resetRoutine = null;
    }

    private void OnDisable()
    {
        if (_resetRoutine != null)
        {
            StopCoroutine(_resetRoutine);
            _resetRoutine = null;
        }
    }
}
