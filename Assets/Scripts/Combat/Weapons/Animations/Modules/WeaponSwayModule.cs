using UnityEngine;

[System.Serializable]
public class WeaponSwayModule : WeaponAnimationModule
{
    // Refreshed
    [SerializeField]
    private WeaponSwayConfigSO weaponSwayConfigSO;

    [Header("Springs")]
    [SerializeField]
    private SpringVector3 swaySpringRotation = new SpringVector3();

    [SerializeField]
    private SpringVector3 swaySpringPosition = new SpringVector3();

    private Vector3 _swayTarget;

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
        if (weaponSwayConfigSO != null)
        {
            swaySpringRotation.SetConstants(weaponSwayConfigSO.stiffness, weaponSwayConfigSO.damping);
            swaySpringPosition.SetConstants(weaponSwayConfigSO.stiffness, weaponSwayConfigSO.damping);

            float breath =
                Mathf.Sin(Time.time * weaponSwayConfigSO.breathFrequency)
                * weaponSwayConfigSO.breathAmplitude;
            swaySpringPosition.SetTarget(new Vector3(0f, breath, 0f));
        }

        swaySpringRotation.SetTarget(_swayTarget);
        
        swaySpringRotation.Update(deltaTime);
        swaySpringPosition.Update(deltaTime);
    }
}
