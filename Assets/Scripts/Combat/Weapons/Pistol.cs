
using UnityEngine;

public class Pistol : WeaponBase
{
    [SerializeField] private Transform muzzlePos;


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
