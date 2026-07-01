using UnityEngine;

[System.Serializable]
public class WeaponInertiaModule : WeaponAnimationModule
{
    //TODO: Add the Profile
    [SerializeField]
    private SpringVector3 _inertiaSpringPosition = new SpringVector3();

    [SerializeField]
    private SpringVector3 _inertiaSpringRotation = new SpringVector3();
    public override Pose AnimationPose =>
        new Pose(_inertiaSpringPosition.Value, Quaternion.Euler(_inertiaSpringRotation.Value));

    [Range(0.01f, 0.1f)]
    [SerializeField]
    private float inertiaAmount = 0.02f;

    [SerializeField]
    private float rollAmount = 10f;

    public void UpdateInertia(Vector3 velocity, Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning(
                "[WeaponInertiaModule] Player transform is not assigned in WeaponInertiaModule."
            );
            return;
        }
        if (velocity.magnitude < 0.5f) // TODO: Make this read something that is not a threshold, like a player state or something
        {
            _inertiaSpringPosition.SetTarget(Vector3.zero);
            _inertiaSpringRotation.SetTarget(Vector3.zero);
            return;
        }
        Vector3 localVelocity = playerTransform.InverseTransformDirection(velocity);
        Vector3 normalizedLocalVelocity = localVelocity.normalized;

        _inertiaSpringPosition.SetTarget(
            new Vector3(
                normalizedLocalVelocity.x * inertiaAmount,
                normalizedLocalVelocity.y * inertiaAmount,
                -normalizedLocalVelocity.z * inertiaAmount
            )
        );
        _inertiaSpringRotation.SetTarget(
            new Vector3(0f, 0f, -rollAmount * normalizedLocalVelocity.x)
        );
    }

    public override void Tick(float deltaTime)
    {
        _inertiaSpringPosition.Update(deltaTime);
        _inertiaSpringRotation.Update(deltaTime);
    }
}
