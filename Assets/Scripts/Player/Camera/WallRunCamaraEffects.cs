using UnityEngine;
using Cinemachine;
public class WallRunCameraEffects : MonoBehaviour
{
    [Header("Cinemachine Reference")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    [Header("Camera Tilt")]
    [SerializeField] private float cameraTilt = 12f;
    [SerializeField] private float tiltSpeed = 8f;
    
    [Header("Field of View")]
    [SerializeField] private float fovIncrease = 10f;
    [SerializeField] private float fovSpeed = 8f;
    
    [Header("Camera Shake (Optional)")]
    [SerializeField] private bool enableShake = true;
    [SerializeField] private float shakeAmplitude = 0.3f;
    [SerializeField] private float shakeFrequency = 1.5f;

    [SerializeField] private PlayerWallRun wallRun;
    private float _defaultFov;
    private float _currentTilt;
    private float _targetTilt;
    private CinemachineBasicMultiChannelPerlin _noise;
    
    // Para el Dutch (Z rotation) de Cinemachine
    private CinemachineComposer _composer;

    private void Awake()
    {

        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("No se encontró CinemachineVirtualCamera. Asigna una en el Inspector.", this);
                enabled = false;
                return;
            }
        }

        // Guardar FOV por defecto
        _defaultFov = virtualCamera.m_Lens.FieldOfView;

        // Obtener o agregar el componente de Noise para shake
        _noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
        // Obtener composer si existe
        _composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
    }

    private void LateUpdate()
    {
        UpdateCameraTilt();
        UpdateFOV();
        UpdateCameraShake();
    }

    private void UpdateCameraTilt()
    {
        // Calcular tilt objetivo basado en el estado del wallrun
        if (wallRun.IsWallRunning)
        {
            _targetTilt = wallRun.IsWallRight ? cameraTilt : -cameraTilt;
        }
        else
        {
            _targetTilt = 0f;
        }

        // Interpolar suavemente
        _currentTilt = Mathf.Lerp(_currentTilt, _targetTilt, Time.deltaTime * tiltSpeed);

        // Aplicar Dutch (rotación en Z) a la cámara virtual
        virtualCamera.m_Lens.Dutch = _currentTilt;
    }

    private void UpdateFOV()
    {
        float targetFov = wallRun.IsWallRunning ? _defaultFov + fovIncrease : _defaultFov;
        
        float currentFov = virtualCamera.m_Lens.FieldOfView;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * fovSpeed);
    }

    private void UpdateCameraShake()
    {
        if (!enableShake || _noise == null) return;

        // Activar shake solo durante wallrun
        if (wallRun.IsWallRunning)
        {
            _noise.m_AmplitudeGain = Mathf.Lerp(_noise.m_AmplitudeGain, shakeAmplitude, Time.deltaTime * 10f);
            _noise.m_FrequencyGain = shakeFrequency;
        }
        else
        {
            _noise.m_AmplitudeGain = Mathf.Lerp(_noise.m_AmplitudeGain, 0f, Time.deltaTime * 10f);
        }
    }

    private void OnDisable()
    {
        // Restaurar valores por defecto
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.FieldOfView = _defaultFov;
            virtualCamera.m_Lens.Dutch = 0f;
            
            if (_noise != null)
            {
                _noise.m_AmplitudeGain = 0f;
            }
        }
    }

    private void OnValidate()
    {
        // Actualizar el FOV por defecto si cambia en el editor
        if (virtualCamera != null && !Application.isPlaying)
        {
            _defaultFov = virtualCamera.m_Lens.FieldOfView;
        }
    }

    // Método público para efectos adicionales si lo necesitas
    public void TriggerImpact(float intensity = 1f)
    {
        if (_noise != null)
        {
            StartCoroutine(ImpactShake(intensity));
        }
    }

    private System.Collections.IEnumerator ImpactShake(float intensity)
    {
        float originalAmplitude = _noise.m_AmplitudeGain;
        _noise.m_AmplitudeGain = shakeAmplitude * intensity * 2f;
        
        yield return new WaitForSeconds(0.1f);
        
        _noise.m_AmplitudeGain = originalAmplitude;
    }
}