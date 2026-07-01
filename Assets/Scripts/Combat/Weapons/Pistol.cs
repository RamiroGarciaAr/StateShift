using UnityEngine;

public class Pistol : WeaponBase
{
    /*
        The Shoot method is responsible for instantiating the projectile and initializing it with the weapon's data.
        It also triggers the OnShoot event, which can be used to trigger the bloom effect on the crosshair or other effects that should happen when the weapon is fired.
        Finally, it consumes ammo from the magazine.
    */
    public override void Shoot()
    {
        GameObject bullet = Instantiate(
            weaponData.ProjectilePrefab,
            muzzlePos.position,
            muzzlePos.rotation
        );
        bullet.GetComponent<ProjectileBase>().Initialise(weaponData);
    }

    public override void Reload()
    {
        // Debug.Log($"Reloading {weaponData.WeaponName}");
    }
}
