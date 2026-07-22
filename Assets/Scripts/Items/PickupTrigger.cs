using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("Total seconds before despawn")]
    private float _lifetime = 8f;

    [SerializeField, Tooltip("Seconds spent blinking before despawn")]
    private float _blinkWindow = 2f;

    [SerializeField, Tooltip("How fast it blinks (seconds per toggle)")]
    private float _blinkInterval = 0.15f;

    [SerializeField]
    private MeshRenderer _renderer; // the thing that blinks
    private IPickupEffect _effect;

    private float _age;
    private bool _collected;
    private bool _released; // guards double-return (collect + expiry on the same frame)

    // Set by the pool on Get so the pickup knows how to return itself.
    private Action<PickupTrigger> _releaseToPool;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true; // attribute guarantees existence; this guarantees trigger

        if (_renderer == null)
        {
            Debug.LogError($"[PickupTrigger] No Mesh on {name}", this);
            enabled = false;
        }

        _effect = GetComponent<IPickupEffect>();
        if (_effect == null)
        {
            Debug.LogError($"[PickupTrigger] No IPickupEffect on {name}", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        _age = 0f;
        _collected = false;
        _released = false;

        // OnEnable can run before Awake on the first activation of a freshly
        // instantiated object, so _renderer may not be assigned yet.
        if (_renderer != null)
            _renderer.enabled = true;
    }

    public void SetReleaseCallback(Action<PickupTrigger> callback)
    {
        _releaseToPool = callback;
    }

    private void Update()
    {
        _age += Time.deltaTime;

        if (_age >= _lifetime)
            Release();
        else if (_age >= _lifetime - _blinkWindow)
            Blink();
    }

    private void Blink()
    {
        _renderer.enabled = (int)(_age / _blinkInterval) % 2 == 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected)
            return;
        if (!other.CompareTag("Player"))
            return;
        if (!_effect.CanApply(other.gameObject))
            return;

        _collected = true;
        _effect.Apply(other.gameObject);
        Release();
    }

    private void Release()
    {
        if (_released)
            return;
        _released = true;

        // Leave the renderer enabled so the object returns to the pool visible,
        // ready for the next spawn (OnEnable also re-enables it defensively).
        _renderer.enabled = true;

        if (_releaseToPool != null)
            _releaseToPool(this);
        else
            gameObject.SetActive(false); // fallback for a pickup placed by hand, outside a pool
    }
}
