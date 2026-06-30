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

    public override Pose AnimationPose =>
        new Pose(_bobSpringPosition.Value, Quaternion.Euler(_bobSpringRotation.Value));

    private float _bobPhase;

    public void UpdateBob(Vector3 worldVelocity, Transform playerTransform)
    {
        if (playerTransform == null || _config == null)
        {
            Debug.LogWarning(
                "[WeaponBobModule] Player transform or WeaponBobConfigSO is not assigned in WeaponBobModule."
            );
            return;
        }

        float speed = worldVelocity.magnitude;

        if (speed > 0.1f)
        {
            _bobPhase += _config.bobFrequency * Time.deltaTime;
            Vector2 lissajousCurve = CalculateLissajous(_bobPhase, 1, 2);

            float verticalBob =
                lissajousCurve.y * _config.bobAmplitude * (speed / _config.bobReferenceSpeed);
            float horizontalBob =
                lissajousCurve.x * _config.bobAmplitude * (speed / _config.bobReferenceSpeed);

            _bobSpringPosition.SetTarget(new Vector3(horizontalBob, verticalBob, 0f));
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

    private Vector2 CalculateLissajous(float t, float amplitude, float height, float delta = 0f)
    {
        return new Vector2(Mathf.Sin(amplitude * t + delta), Mathf.Sin(height * t));
    }
}
