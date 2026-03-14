using Health;
using UnityEngine;
[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Weapon Info")]
    [SerializeField]private string weaponName;
    
    [Header("Weapon Damage")]
    [SerializeField] DamageType damageType;
    [SerializeField] private float damageAmount;
    [SerializeField] private int roundsPerMinute; // how many rounds can be fired in a minute
    
    [Header("Weapon Ammo")]
    [SerializeField]private int magazineSize;
    [SerializeField] private float reloadTime;
    [SerializeField] private int maxAmmo;

    [Header("Weapon Spread")]
    [SerializeField] private float hipFireSpread;
    [SerializeField] private float bloomPerShot;
    [SerializeField] private float bloomRecoveryRate;
    [SerializeField] private float maxBloom;

    [Header("Weapon Projectile")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileLifetime;

    [Header("Damage Over Distance")]
    [SerializeField] private float dropOffMaxRange;
    [SerializeField] private AnimationCurve damageDropOffCurve;
}
