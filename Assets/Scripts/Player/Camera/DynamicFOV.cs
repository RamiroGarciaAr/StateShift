using UnityEngine;

public class DynamicFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField]
    private float baseFOV = 90f;

    [SerializeField]
    private float maxFOVIncrease = 20f;

    [SerializeField]
    private float fovTransitionSpeed = 5f;

    [Header("Speed Settings")]
    [SerializeField]
    private float speedThreshold = 7f;

    [SerializeField]
    private float maxSpeedForFOV = 15f;

    [SerializeField]
    private AnimationCurve fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Advanced")]
    [SerializeField]
    private bool ignoreVerticalVelocity = true;

    [SerializeField]
    private float velocitySmoothing = 0.1f;

    private Rigidbody targetRigidbody;

    [SerializeField]
    private Camera targetCamera;
    private float _targetFOV;
    private float _currentVelocity;
    private float _smoothedSpeed;
    private float _speedVelocity;

    private float _adsWeight;
    private float _adsFOVMultiplier = 1f;
    private float _speedFOV;

    private void Awake()
    {
        //We try to auto-assign the camera and rigidbody if not set, but we log errors if we can't find them so the developer can fix it.
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError(
                    "[DynamicFOV] No camera assigned and Camera.main is null. Assign a camera in the inspector or tag your camera as 'MainCamera'.",
                    this
                );
                enabled = false;
                return;
            }
        }

        var playerGo = GameObject.FindWithTag("Player");
        if (playerGo != null)
        {
            targetRigidbody = playerGo.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogWarning(
                "[DynamicFOV] Could not find Player GameObject with Rigidbody. Dynamic FOV will not work.",
                this
            );
        }
        _targetFOV = baseFOV;
    }

    private void Start()
    {
        _speedFOV = baseFOV;
        targetCamera.fieldOfView = baseFOV;
    }

    private void LateUpdate()
    {
        if (targetRigidbody == null)
            return;
        UpdateFOV();
    }

    private void UpdateFOV()
    {
        Vector3 velocity = targetRigidbody.velocity;

        if (ignoreVerticalVelocity)
        {
            velocity = new Vector3(velocity.x, 0, velocity.z);
        }

        float currentSpeed = velocity.magnitude;

        _smoothedSpeed = Mathf.SmoothDamp(
            _smoothedSpeed,
            currentSpeed,
            ref _speedVelocity,
            velocitySmoothing
        );

        if (_smoothedSpeed < speedThreshold)
        {
            _targetFOV = baseFOV;
        }
        else
        {
            // Calcular velocidad efectiva desde el threshold hasta el máximo
            float effectiveSpeed = _smoothedSpeed - speedThreshold;
            float effectiveMaxSpeed = maxSpeedForFOV - speedThreshold;

            float normalizedSpeed = Mathf.Clamp01(effectiveSpeed / effectiveMaxSpeed);
            float curveValue = fovCurve.Evaluate(normalizedSpeed);

            _targetFOV = baseFOV + (maxFOVIncrease * curveValue);
        }

        // Speed-based FOV is smoothed on its own internal state so the ADS zoom cannot
        // contaminate it. This keeps aiming in and out perfectly symmetric.
        _speedFOV = Mathf.SmoothDamp(
            _speedFOV,
            _targetFOV,
            ref _currentVelocity,
            1f / fovTransitionSpeed
        );

        // ADS zoom is a pure overlay driven only by the hub's symmetric weight.
        float adsFOV = baseFOV * _adsFOVMultiplier;
        targetCamera.fieldOfView = Mathf.Lerp(_speedFOV, adsFOV, _adsWeight);
    }

    /// <summary>
    /// Applies the ADS zoom layer. Driven each frame by the ADS hub so this component
    /// remains the single writer of the camera's field of view.
    /// </summary>
    /// <param name="weight">Eased 0 (hip) to 1 (aimed) blend weight.</param>
    /// <param name="fovMultiplier">Field of view multiplier applied at full ADS.</param>
    public void SetAdsZoom(float weight, float fovMultiplier)
    {
        _adsWeight = Mathf.Clamp01(weight);
        _adsFOVMultiplier = fovMultiplier;
    }

    public void SetBaseFOV(float newBaseFOV)
    {
        baseFOV = newBaseFOV;
    }

    public float GetTargetFOV() => _targetFOV;

    public float GetCurrentFOV()
    {
        return targetCamera.fieldOfView;
    }
}
