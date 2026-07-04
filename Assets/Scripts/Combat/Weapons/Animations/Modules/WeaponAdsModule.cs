using UnityEngine;

/// <summary>
/// Procedural aim-down-sights module. Blends the weapon pivot between a hip pose and an
/// aim pose, and publishes the eased blend weight so decoupled systems (FOV, sensitivity)
/// can react to it.
/// </summary>
[System.Serializable]
public class WeaponAdsModule : WeaponAnimationModule
{
    [Header("Settings")]
    [SerializeField]
    private WeaponAdsConfigSO _config;

    [Header("Shared State")]
    [SerializeField, Tooltip("Eased 0->1 aim weight, published for FOV/sensitivity consumers.")]
    private FloatVariable _adsBlendWeight;

    private bool _isAiming;
    private float _rawWeight;
    private float _easedWeight;
    private Pose _pose = Pose.identity;

    /// <summary>Eased aim weight in the 0 (hip) to 1 (aimed) range.</summary>
    public float Weight => _easedWeight;

    /// <summary>How much sway/breathing should be suppressed at full ADS (0..1).</summary>
    public float SwaySteadiness => _config != null ? _config.SwaySteadiness : 0f;

    /// <summary>How much movement bob should be suppressed at full ADS (0..1).</summary>
    public float BobSteadiness => _config != null ? _config.BobSteadiness : 0f;

    /// <summary>How much movement inertia should be suppressed at full ADS (0..1).</summary>
    public float InertiaSteadiness => _config != null ? _config.InertiaSteadiness : 0f;

    /// <summary>How much visual recoil kick should be suppressed at full ADS (0..1).</summary>
    public float RecoilSteadiness => _config != null ? _config.RecoilSteadiness : 0f;

    /// <summary>How much the movement-state stance offset should be suppressed at full ADS (0..1).</summary>
    public float StanceSteadiness => _config != null ? _config.StanceSteadiness : 0f;

    public override Pose AnimationPose => _pose;

    /// <summary>Resets the blend weight to hip fire. Call from the owner's OnEnable.</summary>
    public void ResetState()
    {
        _isAiming = false;
        _rawWeight = 0f;
        _easedWeight = 0f;

        if (_adsBlendWeight != null)
            _adsBlendWeight.Value = 0f;
    }

    /// <summary>Sets the desired aim state, driven by the aim input event.</summary>
    public void SetAiming(bool isAiming) => _isAiming = isAiming;

    public override void Tick(float deltaTime)
    {
        if (_config == null)
            return;

        float target = _isAiming ? 1f : 0f;
        float step = (1f / _config.BlendDuration) * deltaTime;
        _rawWeight = Mathf.MoveTowards(_rawWeight, target, step);
        _easedWeight = _config.BlendCurve.Evaluate(_rawWeight);

        if (_adsBlendWeight != null)
            _adsBlendWeight.Value = _easedWeight;

        Vector3 position = Vector3.Lerp(_config.HipPosition, _config.AdsPosition, _easedWeight);
        Quaternion rotation = Quaternion.Slerp(
            Quaternion.Euler(_config.HipRotation),
            Quaternion.Euler(_config.AdsRotation),
            _easedWeight
        );

        _pose = new Pose(position, rotation);
    }
}
