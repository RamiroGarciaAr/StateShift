using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : WeaponBase
{
    public override void Shoot()
    {
        GameObject bullet = Instantiate(weaponData.ProjectilePrefab, muzzlePos.position, muzzlePos.rotation);
        bullet.GetComponent<ProjectileBase>().Initialise(weaponData);
    }

    public override void Reload()
    {
        Debug.Log($"Reloading {weaponData.WeaponName}");
    }
}
