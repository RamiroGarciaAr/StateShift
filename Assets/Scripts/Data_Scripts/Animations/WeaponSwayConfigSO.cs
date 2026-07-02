using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSwayConfigSO", menuName = "Animations/Weapon/Sway Config")]
public class WeaponSwayConfigSO : ScriptableObject
{
    [Header("Sway Settings")]
    [
        SerializeField,
        Range(0f, 1f),
        Tooltip("How much the weapon should sway based on the look input")
    ]
    private float _swayAmount = 0.2f;

    [Header("Breath Settings")]
    [
        SerializeField,
        Range(0f, 1f),
        Tooltip("How much the weapon should move up and down based on breathing")
    ]
    private float _breathAmplitude = 0.25f;

    [SerializeField]
    private float _breathRotationPitchAmplitude = 0.5f;

    [SerializeField]
    private float _breathRotationYawAmplitude = 0.5f;

    [SerializeField, Range(0.1f, 10f), Tooltip("How fast the breathing sway should be")]
    private float _breathFrequency = 1f;

    [SerializeField]
    private float _breathRotationPitchFrequency = 1f;

    [SerializeField]
    private float _breathRotationYawFrequency = 1f;

    [Header("Sway Dynamics")]
    [SerializeField, Range(0.5f, 10f)]
    [Tooltip(
        "Frequency (f): Determines the speed at which the system responds to look input. Higher values make the sway snappier."
    )]
    private float _swayF = 2.0f;

    [SerializeField, Range(0f, 2f)]
    [Tooltip(
        "Damping (z): Controls how the sway oscillates. 1 is critical damping (no overshoot), <1 oscillates, >1 is sluggish."
    )]
    private float _swayZ = 0.5f;

    [SerializeField, Range(-2f, 4f)]
    [Tooltip(
        "Initial Response (r): Controls the acceleration or 'kick' of the sway. Positive values cause overshoot/anticipation."
    )]
    private float _swayR = 2.0f;

    [Header("Breathing Dynamics")]
    [SerializeField, Range(0.5f, 10f)]
    [Tooltip("Frequency (f): Determines the speed of the breathing motion response.")]
    private float _breathF = 1.0f;

    [SerializeField, Range(0f, 2f)]
    [Tooltip(
        "Damping (z): Controls the smoothness of the breathing motion. Higher values make it smoother and less bouncy."
    )]
    private float _breathZ = 1.0f;

    [SerializeField, Range(-2f, 4f)]
    [Tooltip("Initial Response (r): Controls the initial reaction to breathing cycles.")]
    private float _breathR = 0f;

    public float SwayAmount => _swayAmount;
    public float BreathAmplitude => _breathAmplitude;
    public float BreathRotationPitchAmplitude => _breathRotationPitchAmplitude;
    public float BreathRotationYawAmplitude => _breathRotationYawAmplitude;
    public float BreathFrequency => _breathFrequency;
    public float BreathRotationPitchFrequency => _breathRotationPitchFrequency;
    public float BreathRotationYawFrequency => _breathRotationYawFrequency;

    public float SwayF => _swayF;
    public float SwayZ => _swayZ;
    public float SwayR => _swayR;
    public float BreathF => _breathF;
    public float BreathZ => _breathZ;
    public float BreathR => _breathR;
}
