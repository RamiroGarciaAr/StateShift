using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponRecoilProfile",
    menuName = "Animations/Weapon/Recoil Profile"
)]
public class WeaponRecoilProfileSO : ScriptableObject
{
    [Header("Rotation Recoil")]
    [Tooltip("Vertical rotation kick (degrees).")]
    [SerializeField]
    private float _recoilAmount = 5f;

    [Tooltip("Random horizontal rotation variation (degrees).")]
    [SerializeField]
    private float _recoilShakeAmount = 2f;

    [Header("Position Recoil")]
    [Tooltip("Z-axis position impulse.")]
    [SerializeField]
    private float _kickbackAmount = 0.1f;

    [Header("Weapon Kick")]
    [Tooltip(
        "Dedicated snappy X-axis (pitch) punch applied to the weapon on fire, layered on top of and independent from the recoil muzzle climb (degrees)."
    )]
    [SerializeField]
    private float _kickAmount = 8f;

    [Tooltip(
        "Random Y-axis (yaw) punch range applied alongside the kick, giving each shot a lively side-to-side snap (degrees)."
    )]
    [SerializeField]
    private float _kickYawAmount = 3f;

    public float RecoilAmount => _recoilAmount;
    public float RecoilShakeAmount => _recoilShakeAmount;
    public float KickbackAmount => _kickbackAmount;

    /// <summary>Magnitude of the dedicated X-axis (pitch) weapon kick punch, in degrees.</summary>
    public float KickAmount => _kickAmount;

    /// <summary>Random Y-axis (yaw) kick range applied per shot, in degrees.</summary>
    public float KickYawAmount => _kickYawAmount;
}
