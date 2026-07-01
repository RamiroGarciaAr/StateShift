using UnityEngine;

[CreateAssetMenu(menuName = ("Animations/Weapon/SwayConfig"))]
public class WeaponSwayConfigSO : ScriptableObject
{
    [Header("Sway Settings")]
    [Tooltip("How much the weapon should sway based on the look input")]
    [Range(0f, 1f)]
    public float swayAmount = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("How much the weapon should move up and down based on breathing")]
    public float breathAmplitude = 0.25f;

    [Range(0.1f, 10f)]
    [Tooltip("How fast the breathing sway should be")]
    public float breathFrequency = 1f;

    [Header("Sway Dynamics")]
    [SerializeField, Range(0.5f, 10f)] private float _swayF = 2.0f;
    [SerializeField, Range(0f, 2f)] private float _swayZ = 0.5f;
    [SerializeField, Range(-2f, 4f)] private float _swayR = 2.0f;

    [Header("Breathing Dynamics")]
    [SerializeField, Range(0.5f, 10f)] private float _breathF = 1.0f;
    [SerializeField, Range(0f, 2f)] private float _breathZ = 1.0f;
    [SerializeField, Range(-2f, 4f)] private float _breathR = 0f;

    public float SwayF => _swayF;
    public float SwayZ => _swayZ;
    public float SwayR => _swayR;
    public float BreathF => _breathF;
    public float BreathZ => _breathZ;
    public float BreathR => _breathR;
}
