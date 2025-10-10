using UnityEngine;
using Cinemachine;

public class DynamicFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField] private float baseFOV = 90f;
    [SerializeField] private float maxFOVIncrease = 20f;
    [SerializeField] private float fovTransitionSpeed = 5f;
    
    [Header("Speed Settings")]
    [SerializeField] private float speedThreshold = 7f;
    [SerializeField] private float maxSpeedForFOV = 15f;
    [SerializeField] private AnimationCurve fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Advanced")]
    [SerializeField] private bool ignoreVerticalVelocity = true;
    [SerializeField] private float velocitySmoothing = 0.1f;
    
    private Rigidbody targetRigidbody;
    private CinemachineVirtualCamera virtualCamera;
    private float _targetFOV;
    private float _currentVelocity;
    private float _smoothedSpeed;
    private float _speedVelocity;

    private void Awake()
    {        
        // Detectar si estamos usando Cinemachine
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        
        var playerGo = GameObject.FindWithTag("Player");
        if (playerGo != null)
        {
            targetRigidbody = playerGo.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogWarning("No se encontró un GameObject con la etiqueta 'Player' para asignar el Rigidbody objetivo.", this);
        }
        _targetFOV = baseFOV;
    }

    private void Start()
    {
        virtualCamera.m_Lens.FieldOfView = baseFOV;
    }

    private void LateUpdate()
    {
        if (targetRigidbody == null) return; 
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
        
        virtualCamera.m_Lens.FieldOfView = Mathf.SmoothDamp(
            virtualCamera.m_Lens.FieldOfView,
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
        return virtualCamera.m_Lens.FieldOfView;
    }

}