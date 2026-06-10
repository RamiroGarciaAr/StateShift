using UnityEngine;

[System.Serializable]
public class WeaponBobModule : WeaponAnimationModule
{
    [Header("Profile")]
    [SerializeField]
    private WeaponBobConfigSO _config;

    [Header("Springs")]
    [SerializeField]
    private SpringVector3 _bobSpringPosition = new SpringVector3();

    [SerializeField]
    private SpringVector3 _bobSpringRotation = new SpringVector3();

    public override Pose AnimationPose => new Pose(_bobSpringPosition.Value, Quaternion.Euler(_bobSpringRotation.Value));

    public void UpdateBob(Vector3 worldVelocity, Transform playerTransform)
    {
        if (playerTransform == null || _config == null) return;

        Vector3 localVelocity = playerTransform.InverseTransformDirection(worldVelocity);
        float speed = worldVelocity.magnitude;

        if (speed > 0.1f)
        {
            float bobPhase = Time.time * _config.bobFrequency;
            
            // Determine relative movement intensity
            float forwardness = localVelocity.z;
            float strafeness = localVelocity.x;

            float verticalBob = Mathf.Sin(bobPhase) * _config.bobAmplitude * (speed / 5f);
            
            _bobSpringPosition.SetTarget(new Vector3(
                strafeness * _config.bobAmplitude, 
                verticalBob, 
                forwardness * _config.bobAmplitude
            ));

            _bobSpringRotation.SetTarget(new Vector3(
                forwardness * _config.bobRotationAmount,
                strafeness * _config.bobRotationAmount,
                -strafeness * _config.bobRotationAmount * 2f // Roll
            ));
        }
        else
        {
            _bobSpringPosition.SetTarget(Vector3.zero);
            _bobSpringRotation.SetTarget(Vector3.zero);
        }
    }

    public override void Tick(float deltaTime)
    {
        if (_config != null)
        {
            _bobSpringPosition.SetConstants(_config.stiffness, _config.damping);
            _bobSpringRotation.SetConstants(_config.stiffness, _config.damping);
        }

        _bobSpringPosition.Update(deltaTime);
        _bobSpringRotation.Update(deltaTime);
    }
}
