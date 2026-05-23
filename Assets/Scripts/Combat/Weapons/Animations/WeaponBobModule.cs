using UnityEngine;

[DefaultExecutionOrder(-20)] // Runs before the WeaponAnimationController to ensure the bob is applied before the animation updates
public class WeaponBobModule : MonoBehaviour
{
    [Header("Bob Settings")]
    [Tooltip("How much the weapon should bob based on the movement input")]
    [Range(0f, 10f)]
    [SerializeField]
    float bobFrequency = 6f;

    [Tooltip("How much the weapon should bob based on the movement input")]
    [Range(0f, 1f)]
    [SerializeField]
    float bobAmplitude = 0.03f;

    [Tooltip("How much the weapon should rotate based on the movement input")]
    [SerializeField]
    float bobRotationAmount = 1f;

    [SerializeField]
    SpringVector3 bobSpringPosition = new SpringVector3();

    [SerializeField]
    SpringVector3 bobSpringRotation = new SpringVector3();

    public Vector3 RotationValue => bobSpringRotation.Value;
    public Vector3 PositionValue => bobSpringPosition.Value;

    private Vector2 _moveInput;

    private void LateUpdate()
    {
        if (_moveInput.sqrMagnitude > 0.01f)
        {
            float bobPhase = Time.time * bobFrequency;
            float forwardness = Mathf.Abs(_moveInput.y);
            float strafeness = Mathf.Abs(_moveInput.x);
            float verticalBob =
                Mathf.Sin(bobPhase) * bobAmplitude * Mathf.Max(forwardness, strafeness * 0.5f);
            // position bob
            bobSpringPosition.SetTarget(
                new Vector3(strafeness * bobAmplitude, verticalBob, _moveInput.y * bobAmplitude)
            );
            // rotation bob
            float strafeRoll = -_moveInput.x * bobRotationAmount * 2f;
            bobSpringRotation.SetTarget(
                new Vector3(
                    _moveInput.y * bobRotationAmount,
                    _moveInput.x * bobRotationAmount,
                    strafeRoll
                )
            );
        }
        else
        {
            bobSpringPosition.SetTarget(Vector3.zero);
            bobSpringRotation.SetTarget(Vector3.zero);
        }

        bobSpringPosition.Update(Time.deltaTime);
        bobSpringRotation.Update(Time.deltaTime);
    }

    public void ApplyBob(Vector2 moveInput) => _moveInput = moveInput;
}
