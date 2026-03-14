using Health;
using UnityEngine;
[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Weapon Info")]
    [SerializeField] private string weaponName;

    public string WeaponName => weaponName;
    [Header("Damage")]
    [SerializeField] private DamageType damageType;
    [SerializeField] private float damageAmount;

    public DamageType DamageType => damageType;
    public float DamageAmount => damageAmount;
    [Header("Fire Rate")]
    [SerializeField] private int roundsPerMinute;

    public int RoundsPerMinute => roundsPerMinute;
    public float SecondsBetweenShots => 60f / roundsPerMinute;
    [Header("Ammo")]
    [SerializeField] private int magazineSize;
    [SerializeField] private float reloadTime;

    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;

    [Header("Spread & Bloom")]
    [SerializeField] private float hipFireSpread;
    [SerializeField] private float adsSpreadMultiplier;  // e.g. 0.3 → 70% tighter in ADS
    [SerializeField] private float bloomPerShot;
    [SerializeField] private float bloomRecoveryRate;
    [SerializeField] private float maxBloom;

    public float HipFireSpread => hipFireSpread;
    public float AdsSpreadMultiplier => adsSpreadMultiplier;
    public float BloomPerShot => bloomPerShot;
    public float BloomRecoveryRate => bloomRecoveryRate;
    public float MaxBloom => maxBloom;
    [Header("ADS Behaviour")]
    [SerializeField] private float adsMovementMultiplier;   // e.g. 0.65
    [SerializeField] private bool blockAdsWhileWallRunning;

    public float AdsMovementMultiplier => adsMovementMultiplier;
    public bool BlockAdsWhileWallRunning => blockAdsWhileWallRunning;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileLifetime;

    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;

    [Header("Damage Falloff")]
    [SerializeField] private float dropOffMaxRange;
    [SerializeField] private AnimationCurve damageDropOffCurve;

    public float DropOffMaxRange => dropOffMaxRange;
    public AnimationCurve DamageDropOffCurve => damageDropOffCurve;


    /// <summary>
    /// Returns the damage multiplier at a given world-space distance.
    /// Safe to call with distance > dropOffMaxRange — curve clamps at 1.0 on X axis.
    /// </summary>
    public float GetDamageMultiplierAtDistance(float distance)
    {
        if (dropOffMaxRange <= 0f) return 1f; 
        float normalised = Mathf.Clamp01(distance / dropOffMaxRange);
        return damageDropOffCurve.Evaluate(normalised);
    }
}