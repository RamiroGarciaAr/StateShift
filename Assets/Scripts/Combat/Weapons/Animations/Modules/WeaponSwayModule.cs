using UnityEngine;

[System.Serializable]
public class WeaponSwayModule : WeaponAnimationModule
{
    // Refreshed
    [SerializeField]
    private WeaponSwayConfigSO weaponSwayConfigSO;

    [Header("Dynamics")]
    [SerializeField]
    private SecondOrderDynamics swaySpringRotation;

    [SerializeField]
    private SecondOrderDynamics swaySpringPosition;

    [Header("Smoothed Look Input")]
    [
        Range(10f, 30f),
        Tooltip("How quickly the look input is smoothed. Higher values result in faster smoothing.")
    ]
    private float _lookSmoothing = 15f;

    private Vector2 _rawLook;
    private Vector3 _swayTarget;
    private Vector2 _smoothedLook;
    private bool _initialized;

    public override Pose AnimationPose =>
        new Pose(swaySpringPosition.Value, Quaternion.Euler(swaySpringRotation.Value));

    public void ApplySway(Vector2 lookInput)
    {
        _rawLook = lookInput;
    }

    public override void Tick(float deltaTime)
    {
        if (weaponSwayConfigSO == null)
            return;

        swaySpringRotation.ComputeConstants(
            weaponSwayConfigSO.SwayF,
            weaponSwayConfigSO.SwayZ,
            weaponSwayConfigSO.SwayR
        );
        swaySpringPosition.ComputeConstants(
            weaponSwayConfigSO.BreathF,
            weaponSwayConfigSO.BreathZ,
            weaponSwayConfigSO.BreathR
        );

        float breath = Breathe(
            weaponSwayConfigSO.BreathFrequency,
            weaponSwayConfigSO.BreathAmplitude
        );
        Vector3 breathTarget = new Vector3(0f, breath, 0f);
        float breathPitch = Breathe(
            weaponSwayConfigSO.BreathRotationPitchFrequency,
            weaponSwayConfigSO.BreathRotationPitchAmplitude
        );
        float breathYaw = Breathe(
            weaponSwayConfigSO.BreathRotationYawFrequency,
            weaponSwayConfigSO.BreathRotationYawAmplitude
        );
        _smoothedLook = Vector2.Lerp(
            _smoothedLook,
            _rawLook,
            1f - Mathf.Exp(-_lookSmoothing * deltaTime)
        );
        _swayTarget = new Vector3(
            -_smoothedLook.y * weaponSwayConfigSO.SwayAmount,
            _smoothedLook.x * weaponSwayConfigSO.SwayAmount,
            0f
        );

        Vector3 rotationTarget = _swayTarget + new Vector3(breathPitch, breathYaw, 0f);

        if (!_initialized)
        {
            swaySpringRotation.Initialize(rotationTarget);
            swaySpringPosition.Initialize(breathTarget);
            _initialized = true;
        }

        swaySpringRotation.Update(deltaTime, rotationTarget);
        swaySpringPosition.Update(deltaTime, breathTarget);
    }

    private float Breathe(float frequency, float amplitude) =>
        Mathf.Sin(Time.time * frequency) * amplitude;
}
