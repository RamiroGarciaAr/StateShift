using UnityEngine;

[System.Serializable]
public class WeaponInertiaModule : WeaponAnimationModule
{
    [Header("Settings")]
    [SerializeField]
    private WeaponInertiaConfigSO _config;

    [Header("Dynamics")]
    [SerializeField]
    private SpringVector3 _inertiaSpringPosition = new SpringVector3();

    [SerializeField]
    private SecondOrderDynamics _inertiaSpringRotation;

    private Vector3 _rollTarget;
    private bool _initialized;

    public override Pose AnimationPose =>
        new Pose(_inertiaSpringPosition.Value, Quaternion.Euler(_inertiaSpringRotation.Value));

    public void UpdateInertia(Vector3 velocity, Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning(
                "[WeaponInertiaModule] Player transform is not assigned in WeaponInertiaModule."
            );
            return;
        }

        if (_config == null)
            return;

        // Preserve existing velocity threshold logic exactly as written
        if (velocity.magnitude < 0.5f)
        {
            _inertiaSpringPosition.SetTarget(Vector3.zero);
            _rollTarget = Vector3.zero;
            return;
        }

        Vector3 localVelocity = playerTransform.InverseTransformDirection(velocity);
        Vector3 normalizedLocalVelocity = localVelocity.normalized;

        _inertiaSpringPosition.SetTarget(
            new Vector3(
                normalizedLocalVelocity.x * _config.InertiaAmount,
                normalizedLocalVelocity.y * _config.InertiaAmount,
                -normalizedLocalVelocity.z * _config.InertiaAmount
            )
        );

        _rollTarget = new Vector3(0f, 0f, -_config.RollAmount * normalizedLocalVelocity.x);
    }

    public override void Tick(float deltaTime)
    {
        if (_config == null)
            return;

        // Rotation SOD logic
        _inertiaSpringRotation.ComputeConstants(_config.RollF, _config.RollZ, _config.RollR);

        if (!_initialized)
        {
            _inertiaSpringRotation.Initialize(Vector3.zero);
            _initialized = true;
        }

        _inertiaSpringRotation.Update(deltaTime, _rollTarget);

        // Position Spring logic
        _inertiaSpringPosition.Update(deltaTime);
    }
}
