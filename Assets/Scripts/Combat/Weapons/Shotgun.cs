using UnityEngine;

public class Shotgun : WeaponBase
{
    [SerializeField] int pelletCount=1;
    public override void Shoot()
    {
        float halfSpread = weaponData.SpreadAngle *0.5f;
        for (int i=0; i<pelletCount;i++)
        {
            Quaternion spread = Quaternion.Euler(Random.Range(-halfSpread, halfSpread), Random.Range(-halfSpread, halfSpread),0f);

            Quaternion pelletRotation = muzzlePos.rotation * spread;

            GameObject bullet = Instantiate(weaponData.ProjectilePrefab, muzzlePos.position, pelletRotation);
            bullet.GetComponent<ProjectileBase>().Initialise(weaponData);
        }

    }

    public override void Reload()
    {
        // Debug.Log($"Reloading {weaponData.WeaponName}");
    }
}
