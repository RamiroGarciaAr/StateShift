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

    private Vector3 _swayTarget;
    private bool _initialized;

    public override Pose AnimationPose =>
        new Pose(swaySpringPosition.Value, Quaternion.Euler(swaySpringRotation.Value));

    public void ApplySway(Vector2 lookInput)
    {
        if (weaponSwayConfigSO == null)
            return;

        _swayTarget = new Vector3(
            -lookInput.y * weaponSwayConfigSO.SwayAmount,
            lookInput.x * weaponSwayConfigSO.SwayAmount,
            0f
        );
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
