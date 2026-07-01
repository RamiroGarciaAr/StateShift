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
        if (weaponSwayConfigSO == null) return;

        _swayTarget = new Vector3(
            -lookInput.y * weaponSwayConfigSO.swayAmount,
            lookInput.x * weaponSwayConfigSO.swayAmount,
            0f
        );
    }

    public override void Tick(float deltaTime)
    {
        if (weaponSwayConfigSO == null) return;

        swaySpringRotation.ComputeConstants(weaponSwayConfigSO.SwayF, weaponSwayConfigSO.SwayZ, weaponSwayConfigSO.SwayR);
        swaySpringPosition.ComputeConstants(weaponSwayConfigSO.BreathF, weaponSwayConfigSO.BreathZ, weaponSwayConfigSO.BreathR);

        float breath =
            Mathf.Sin(Time.time * weaponSwayConfigSO.breathFrequency)
            * weaponSwayConfigSO.breathAmplitude;
        Vector3 breathTarget = new Vector3(0f, breath, 0f);

        if (!_initialized)
        {
            swaySpringRotation.Initialize(_swayTarget);
            swaySpringPosition.Initialize(breathTarget);
            _initialized = true;
        }

        swaySpringRotation.Update(deltaTime, _swayTarget);
        swaySpringPosition.Update(deltaTime, breathTarget);
    }
}
