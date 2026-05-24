using UnityEngine;

public class WeaponSwayModule : MonoBehaviour, IAnimationModule
{
    [SerializeField]
    private WeaponSwayConfigSO weaponSwayConfigSO;

    [Header("Springs")]
    //TODO: Springs could be SO so they can be changed at run time too
    [Tooltip("How much the weapon should sway based on the look input")]
    [SerializeField]
    private SpringVector3 swaySpringRotation = new SpringVector3();

    [SerializeField]
    private SpringVector3 swaySpringPosition = new SpringVector3();

    private Vector3 _swayTarget;

    public Pose AnimationPose =>
        new Pose(swaySpringPosition.Value, Quaternion.Euler(swaySpringRotation.Value));

    public void ApplySway(Vector2 lookInput)
    {
        Debug.Log(lookInput);
        _swayTarget = new Vector3(
            -lookInput.y * weaponSwayConfigSO.swayAmount,
            lookInput.x * weaponSwayConfigSO.swayAmount,
            0f
        );
    }

    public void Tick(float deltaTime)
    {
        Debug.Log("Tick");
        swaySpringRotation.SetTarget(_swayTarget);
        // Breathing drives position independently
        float breath =
            Mathf.Sin(Time.time * weaponSwayConfigSO.breathFrequency)
            * weaponSwayConfigSO.breathAmplitude;
        swaySpringPosition.SetTarget(new Vector3(0f, breath, 0f));

        swaySpringRotation.Update(deltaTime);
        swaySpringPosition.Update(deltaTime);
    }
}
