using Combat.FireModes;
using Health;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Weapon Info")]
    [SerializeField]
    private string weaponName;

    [SerializeField]
    private GameObject weaponPrefab;

    [SerializeField]
    private FireModeType[] availableFireModes;

    [SerializeField]
    private int burstCount; // Only used if the weapon has a burst fire mode

    [Header("Weapon Accuracy")]
    [SerializeField]
    private float spreadAngle;

    public float SpreadAngle => spreadAngle;
    public string WeaponName => weaponName;

    public FireModeType[] AvailableFireModes => availableFireModes;
    public int BurstCount => burstCount;

    [Header("Damage")]
    [SerializeField]
    private DamageType damageType;

    [SerializeField]
    private float damageAmount;

    [Header("Damage Drop Off")]
    [SerializeField]
    private DamageDropoff damageDropoff;

    public DamageType DamageType => damageType;
    public float DamageAmount => damageAmount;

    [Header("Fire Rate")]
    [SerializeField]
    private int roundsPerMinute;
    public int RoundsPerMinute => roundsPerMinute;
    public float SecondsBetweenShots => 60f / roundsPerMinute;

    [Header("Ammo")]
    [SerializeField]
    private int magazineSize;

    [SerializeField]
    private float reloadTime;

    [SerializeField]
    private int totalAmmo; // Total ammo that the weapon can hold, does not include the ammo currently in the magazine

    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public int TotalAmmo => totalAmmo;

    [Header("Projectile")]
    [SerializeField]
    private GameObject projectilePrefab; //TODO: This should be a pool of projectiles instead of a gameobject

    [SerializeField]
    private float projectileSpeed;

    [SerializeField]
    private float projectileLifetime;

    [Header("Sounds")]
    [SerializeField]
    private Sound fireSounds;

    public Sound FireSounds => fireSounds;

    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;

    public float GetDamageMultiplierAtDistance(float distance) =>
        damageDropoff.GetDamageMultiplierAtDistance(distance);

    private void OnValidate()
    {
        bool hasBurst = false;
        foreach (var fireMode in availableFireModes)
        {
            if (fireMode == FireModeType.Burst)
            {
                hasBurst = true;
                if (burstCount <= 0)
                {
                    burstCount = 3; // Default burst count
                    Debug.LogWarning(
                        $"Burst count must be greater than 0 for weapon {weaponName}. Setting to default value of 3."
                    );
                }
            }
        }
        //TODO: We will change this system to handle more firemodes better
        if (!hasBurst)
            burstCount = 0; // If the weapon doesn't have burst fire mode, we set the burst count to 0 to avoid confusion
    }
}
