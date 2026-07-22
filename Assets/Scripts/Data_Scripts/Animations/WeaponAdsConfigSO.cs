using UnityEngine;

[CreateAssetMenu(fileName = "WeaponAdsConfigSO", menuName = "Animations/Weapon/Ads Config")]
public class WeaponAdsConfigSO : ScriptableObject
{
    [Header("Hip Pose (weight = 0)")]
    [SerializeField, Tooltip("Local position offset of the weapon pivot while hip firing.")]
    private Vector3 _hipPosition = Vector3.zero;

    [SerializeField, Tooltip("Local euler rotation offset of the weapon pivot while hip firing.")]
    private Vector3 _hipRotation = Vector3.zero;

    [Header("Aim Pose (weight = 1)")]
    [SerializeField, Tooltip("Local position offset that aligns the sight with the camera center.")]
    private Vector3 _adsPosition = Vector3.zero;

    [SerializeField, Tooltip("Local euler rotation offset applied when fully aimed.")]
    private Vector3 _adsRotation = Vector3.zero;

    [Header("Blend")]
    [SerializeField, Range(0.01f, 1f), Tooltip("Seconds to blend fully between hip and aimed.")]
    private float _blendDuration = 0.2f;

    [
        SerializeField,
        Tooltip("Shapes the 0->1 blend. EaseInOut gives a snappy, overshoot-free feel.")
    ]
    private AnimationCurve _blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Steadiness (0 = unchanged, 1 = fully suppressed at full ADS)")]
    [
        SerializeField,
        Range(0f, 1f),
        Tooltip("How much weapon sway/breathing is reduced while aiming.")
    ]
    private float _swaySteadiness = 0.8f;

    [SerializeField, Range(0f, 1f), Tooltip("How much movement bob is reduced while aiming.")]
    private float _bobSteadiness = 0.7f;

    [SerializeField, Range(0f, 1f), Tooltip("How much movement inertia is reduced while aiming.")]
    private float _inertiaSteadiness = 0.6f;

    [SerializeField, Range(0f, 1f), Tooltip("How much visual recoil kick is reduced while aiming.")]
    private float _recoilSteadiness = 0.25f;

    [
        SerializeField,
        Range(0f, 1f),
        Tooltip(
            "How much the movement-state stance offset is reduced while aiming. Keep at 1 so the sight aligns exactly with the camera at full ADS."
        )
    ]
    private float _stanceSteadiness = 1f;

    public Vector3 HipPosition => _hipPosition;
    public Vector3 HipRotation => _hipRotation;
    public Vector3 AdsPosition => _adsPosition;
    public Vector3 AdsRotation => _adsRotation;

    public float BlendDuration => _blendDuration;
    public AnimationCurve BlendCurve => _blendCurve;

    public float SwaySteadiness => _swaySteadiness;
    public float BobSteadiness => _bobSteadiness;
    public float InertiaSteadiness => _inertiaSteadiness;
    public float RecoilSteadiness => _recoilSteadiness;
    public float StanceSteadiness => _stanceSteadiness;
}
