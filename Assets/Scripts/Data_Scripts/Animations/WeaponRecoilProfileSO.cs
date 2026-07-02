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

    public float RecoilAmount => _recoilAmount;
    public float RecoilShakeAmount => _recoilShakeAmount;
    public float KickbackAmount => _kickbackAmount;
}
