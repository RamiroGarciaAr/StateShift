using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBobConfig", menuName = "Animations/Weapon/Bob Config")]
public class WeaponBobConfigSO : ScriptableObject
{
    [Header("Frequencies")]
    public float bobFrequency = 6f;
    public float bobRotationAmount = 1f;
    public float bobAmplitude = 0.03f;
    public float bobReferenceSpeed = 5f;

    [Header("Spring Settings")]
    public float stiffness = 150f;
    public float damping = 15f;
}
