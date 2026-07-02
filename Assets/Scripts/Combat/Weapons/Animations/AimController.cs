using Entities.Controllers;
using UnityEngine;

/// <summary>
/// Camera-facing hub for aim-down-sights effects. Reads the shared ADS blend weight
/// (written by <see cref="WeaponAdsModule"/>) and drives FOV zoom and aim sensitivity,
/// keeping those effects perfectly in sync with the weapon pose.
/// </summary>
public class AimController : MonoBehaviour
{
    private const float MinAdsFovMultiplier = 0.3f;
    private const float MaxAdsFovMultiplier = 1f;

    [Header("Shared State")]
    [SerializeField, Tooltip("Eased 0->1 ADS weight, published by the weapon ADS module.")]
    private FloatVariable _adsBlendWeight;

    [Header("FOV Zoom")]
    [SerializeField, Tooltip("The camera FOV driver that owns the final field-of-view write.")]
    private DynamicFOV _dynamicFov;

    [
        SerializeField,
        Range(MinAdsFovMultiplier, MaxAdsFovMultiplier),
        Tooltip("Field of view multiplier at full ADS. Lower values zoom in more (more punch).")
    ]
    private float _adsFovMultiplier = 0.55f;

    [
        SerializeField,
        Range(0.5f, 20f),
        Tooltip("ADS zoom transition speed in weight units/second. Identical for aiming in and out; a full transition takes ~1/value seconds.")
    ]
    private float _adsZoomSpeed = 8f;

    [Header("Sensitivity")]
    [
        SerializeField,
        Range(0.1f, 1f),
        Tooltip("Aim look sensitivity multiplier at full ADS, for slower, steadier aiming.")
    ]
    private float _adsSensitivityMultiplier = 0.6f;

    [Header("Crosshair")]
    [SerializeField, Tooltip("Crosshair canvas group; hidden instantly the moment the aim key is pressed.")]
    private CanvasGroup _crosshairGroup;

    /// <summary>Current aim look sensitivity multiplier for the active blend weight.</summary>
    public float SensitivityMultiplier { get; private set; } = 1f;

    private float _smoothedZoomWeight;

    private void OnEnable()
    {
        PlayerInput.OnAim += HandleAim;
    }

    private void OnDisable()
    {
        PlayerInput.OnAim -= HandleAim;
        SetCrosshairVisible(true);
    }

    private void Update()
    {
        if (_adsBlendWeight == null)
            return;

        float weight = _adsBlendWeight.Value;

        // Constant-rate move so aiming in and out take the exact same time.
        _smoothedZoomWeight = Mathf.MoveTowards(
            _smoothedZoomWeight,
            weight,
            _adsZoomSpeed * Time.deltaTime
        );

        if (_dynamicFov != null)
            _dynamicFov.SetAdsZoom(_smoothedZoomWeight, _adsFovMultiplier);

        SensitivityMultiplier = Mathf.Lerp(1f, _adsSensitivityMultiplier, weight);
    }

    private void HandleAim(bool isAiming) => SetCrosshairVisible(!isAiming);

    private void SetCrosshairVisible(bool isVisible)
    {
        if (_crosshairGroup != null)
            _crosshairGroup.alpha = isVisible ? 1f : 0f;
    }
}
