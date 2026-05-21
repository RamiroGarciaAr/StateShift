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

        targetCamera.fieldOfView = Mathf.SmoothDamp(
            targetCamera.fieldOfView,
            _targetFOV,
            ref _currentVelocity,
            1f / fovTransitionSpeed
        );
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
