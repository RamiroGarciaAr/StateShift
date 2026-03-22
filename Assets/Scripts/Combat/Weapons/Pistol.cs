using System.Collections;
using System.Collections.Generic;
using Entities.Controllers;
using UnityEngine;

public class Pistol : WeaponBase
{
    [SerializeField] private Transform muzzlePos;

    void Start()
    {
        PlayerController.OnShoot += Shoot;
    }
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
