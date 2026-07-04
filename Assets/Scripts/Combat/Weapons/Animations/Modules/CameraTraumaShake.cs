using UnityEngine;

/// <summary>
/// Applies a short, decaying, high-frequency trauma-squared-plus-noise jitter as an additive
/// local rotation on its own dedicated pivot, triggered once per shot. The shake is orthogonal
/// to the deterministic recoil owned by <see cref="CameraRecoilModule"/>; it composes on top of
/// recoil because this pivot sits below the recoil pivot. Magnitude scales down (but is never
/// zeroed) during ADS. Rotation only, never position.
/// </summary>
// [DefaultExecutionOrder(-20)] is precautionary: order between the exclusive-owner camera
// pivots is currently irrelevant since each owns its own transform.
[DefaultExecutionOrder(-20)]
public class CameraTraumaShake : MonoBehaviour
{
    [Header("Trauma")]
    [Tooltip("Trauma kick added per shot. Small values let full-auto ramp toward the clamp.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _traumaPerShot = 0.25f;

    [Tooltip("Linear trauma decay toward 0 per second.")]
    [Range(0.1f, 6f)]
    [SerializeField]
    private float _traumaDecayPerSecond = 1.8f;

    [Tooltip("Trauma exponent. 2 = punchy-then-smooth, 3 = snappier with less lingering.")]
    [Range(1f, 3f)]
    [SerializeField]
    private float _traumaExponent = 2f;

    [Header("Noise")]
    [Tooltip("Perlin noise scroll speed for the high-frequency jitter.")]
    [Range(1f, 60f)]
    [SerializeField]
    private float _noiseFrequency = 28f;

    [Tooltip("Max pitch/yaw/roll magnitude in degrees. Roll-heavy reads most like MW.")]
    [SerializeField]
    private Vector3 _maxAnglesDegrees = new Vector3(2f, 2f, 3f);

    [Header("ADS")]
    [Tooltip("Shared 0..1 ADS blend weight written by WeaponAdsModule.")]
    [SerializeField]
    private FloatVariable _adsBlendWeight;

    [Tooltip("Magnitude fraction at full ADS. Never 0 so sights still feel alive.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _adsShakeScale = 0.3f;

    // Runtime state only, never serialized.
    private CameraTraumaShakeState _shakeState;
    private Transform _cachedTransform;
    private bool _wasAtRest;

    private void Awake()
    {
        _cachedTransform = transform;
        _shakeState = new CameraTraumaShakeState();
    }

    private void OnEnable()
    {
        WeaponBase.OnShoot += HandleShoot;
    }

    private void OnDisable()
    {
        WeaponBase.OnShoot -= HandleShoot;
    }

    private void LateUpdate()
    {
        // TODO: bullet-time -- swap to unscaled here.
        float deltaTime = Time.deltaTime;

        _shakeState.Tick(_traumaDecayPerSecond, _noiseFrequency, deltaTime);

        if (_shakeState.IsAtRest)
        {
            if (!_wasAtRest)
            {
                _cachedTransform.localRotation = Quaternion.identity;
                _wasAtRest = true;
            }

            return;
        }

        _wasAtRest = false;

        float adsWeight = _adsBlendWeight != null ? _adsBlendWeight.Value : 0f;
        float adsScale = Mathf.Lerp(1f, _adsShakeScale, adsWeight);
        Vector3 offset = _shakeState.GetRotationOffset(_maxAnglesDegrees, _traumaExponent) * adsScale;

        if (float.IsNaN(offset.x) || float.IsNaN(offset.y) || float.IsNaN(offset.z))
        {
            return;
        }

        _cachedTransform.localRotation = Quaternion.Euler(offset);
    }

    private void HandleShoot()
    {
        // Facts only: one shot = one trauma kick. Nothing time-based lives here.
        _shakeState.AddTrauma(_traumaPerShot);
    }
}
