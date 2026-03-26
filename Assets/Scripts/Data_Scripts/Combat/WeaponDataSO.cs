using Health;
using UnityEngine;
[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Weapon Info")]
    [SerializeField] private string weaponName;
    [SerializeField] private GameObject weaponPrefab;

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
    [SerializeField] private int totalAmmo; // Total ammo that the weapon can hold, does not include the ammo currently in the magazine

    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public int TotalAmmo => totalAmmo;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileLifetime;

    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;

    //TODO: I hate Using the Animation Curve So we need to change this to something else but for now it works and is easy to edit in the inspector so we will keep it for now
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