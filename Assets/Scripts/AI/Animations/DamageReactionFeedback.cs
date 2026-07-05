using System;
using Health;
using UnityEngine;

[RequireComponent(typeof(BaseHealth), typeof(Animator))]
public class DamageReactionFeedback : MonoBehaviour
{
    private Animator _animator;
    private BaseHealth _baseHealth;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _baseHealth = GetComponent<BaseHealth>();

        if (_animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        if (_baseHealth == null)
        {
            Debug.LogError("BaseHealth component not found on " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        if (_baseHealth != null)
        {
            _baseHealth.OnHealthChanged += OnHandleHitAnimation;
            _baseHealth.OnDeath += OnHandleDeathAnimation;
        }
    }

    private void OnHandleHitAnimation(HealthChangeEventArgs args)
    {
        // If we take damage
        if (args.DamageDealt > 0)
        {
            _animator.SetTrigger("HasHit");
        }
    }

    private void OnHandleDeathAnimation()
    {
        _animator.SetTrigger("HasDied");
    }

    private void OnDisable()
    {
        if (_baseHealth != null)
        {
            _baseHealth.OnHealthChanged -= OnHandleHitAnimation;
            _baseHealth.OnDeath -= OnHandleDeathAnimation;
        }
    }
}
