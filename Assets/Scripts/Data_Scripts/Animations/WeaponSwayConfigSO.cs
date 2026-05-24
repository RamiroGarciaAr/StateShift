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
}
