using UnityEngine;

[System.Serializable]
public class WeaponSwayModule
{
    [Header("Sway Settings")]
    [Tooltip("How much the weapon should sway based on the look input")]
    [Range(0f, 1f)]
    [SerializeField]
    private float swayAmount = 0.2f;

    [Tooltip("How much the weapon should sway based on the look input")]
    [SerializeField]
    private SpringVector3 swaySpringRotation = new SpringVector3();

    [Range(0f, 1f)]
    [Tooltip("How much the weapon should move up and down based on breathing")]
    [SerializeField]
    private float breathAmplitude = 0.25f;

    [Range(0.1f, 10f)]
    [Tooltip("How fast the breathing sway should be")]
    [SerializeField]
    private float breathFrequency = 1f;

    [SerializeField]
    private SpringVector3 swaySpringPosition = new SpringVector3();
    public Vector3 RotationValue => swaySpringRotation.Value;
    public Vector3 PositionValue => swaySpringPosition.Value;

    private Vector3 _swayTarget;

    public void ApplySway(Vector2 lookInput)
    {
        _swayTarget = new Vector3(-lookInput.y * swayAmount, lookInput.x * swayAmount, 0f);
    }

    public void LateUpdate()
    {
        swaySpringRotation.SetTarget(_swayTarget);
        // Breathing drives position independently
        float breath = Mathf.Sin(Time.time * breathFrequency) * breathAmplitude;
        swaySpringPosition.SetTarget(new Vector3(0f, breath, 0f));

        swaySpringRotation.Update(Time.deltaTime);
        swaySpringPosition.Update(Time.deltaTime);
    }
}
