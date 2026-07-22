using System;
using System.Collections;
using Health;
using UnityEngine;

[RequireComponent(typeof(BaseHealth), typeof(Animator))]
public class RespawnOnDeath : MonoBehaviour
{
    [Tooltip("The Time that the target waits before getting back up")]
    [SerializeField, Range(1f, 20f)]
    private float respawnTimer = 10f;
    private BaseHealth _baseHealth;
    private Animator _animator;
    private float maxHealth;

    void Awake()
    {
        _baseHealth = GetComponent<BaseHealth>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (_baseHealth != null)
            maxHealth = _baseHealth.MaxHealth;
    }

    void OnEnable()
    {
        _baseHealth.OnDeath += StartRespawn;
    }

    void OnDisable()
    {
        _baseHealth.OnDeath -= StartRespawn;
        StopCoroutine(ResetAfterDelay());
    }

    private void StartRespawn()
    {
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(respawnTimer);
        ResetTarget();
    }

    void ResetTarget()
    {
        _baseHealth.Heal(maxHealth);
        _animator.SetTrigger("Reset");
        _animator.ResetTrigger("HasDied");
    }
}
