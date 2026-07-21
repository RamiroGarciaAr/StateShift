using UnityEngine;

/// <summary>
/// Applies a short fixed camera shake on an exclusive additive camera pivot.
/// </summary>
public class DamageCameraShake : MonoBehaviour
{
    [Header("Impulse")]
    [Tooltip("Duration in seconds of each fixed incoming-damage camera impulse.")]
    [SerializeField, Min(0.01f)] private float _duration = 0.12f;

    [Tooltip("Maximum local positional offset in metres at the beginning of the impulse.")]
    [SerializeField, Min(0f)] private float _positionAmplitude = 0.018f;

    [Tooltip("Maximum local rotational offset in degrees at the beginning of the impulse.")]
    [SerializeField, Min(0f)] private float _rotationAmplitude = 0.7f;

    [Tooltip("Oscillation frequency used to shape the short impulse.")]
    [SerializeField, Min(0f)] private float _frequency = 26f;

    private Vector3 _restLocalPosition;
    private Quaternion _restLocalRotation;
    private float _remainingTime;

    private void Awake()
    {
        _restLocalPosition = transform.localPosition;
        _restLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (_remainingTime <= 0f)
            return;

        float duration = Mathf.Max(0.01f, _duration);
        _remainingTime = Mathf.Max(0f, _remainingTime - Time.deltaTime);
        float normalizedTime = 1f - (_remainingTime / duration);
        float envelope = 1f - normalizedTime;
        float oscillation = Mathf.Sin(normalizedTime * _frequency);

        Vector3 positionOffset = new Vector3(
            oscillation * _positionAmplitude * envelope,
            Mathf.Cos(normalizedTime * _frequency * 1.37f) * _positionAmplitude * 0.6f * envelope,
            0f
        );
        Vector3 rotationOffset = new Vector3(
            -oscillation * _rotationAmplitude * envelope,
            oscillation * _rotationAmplitude * 0.45f * envelope,
            0f
        );

        transform.localPosition = _restLocalPosition + positionOffset;
        transform.localRotation = _restLocalRotation * Quaternion.Euler(rotationOffset);

        if (_remainingTime <= 0f)
        {
            transform.localPosition = _restLocalPosition;
            transform.localRotation = _restLocalRotation;
        }
    }

    /// <summary>
    /// Restarts the configured fixed incoming-damage camera impulse.
    /// </summary>
    public void PlayShake()
    {
        _remainingTime = Mathf.Max(0.01f, _duration);
    }
}
