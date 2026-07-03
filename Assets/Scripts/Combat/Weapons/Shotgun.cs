using UnityEngine;

public class Shotgun : WeaponBase
{
    [SerializeField] private int pelletCount = 1;

    public override void Shoot()
    {
        float halfSpread = weaponData.SpreadAngle * 0.5f;
        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion spread = Quaternion.Euler(Random.Range(-halfSpread, halfSpread), Random.Range(-halfSpread, halfSpread), 0f);
            Quaternion pelletRotation = muzzlePos.rotation * spread;
            SpawnProjectile(muzzlePos.position, pelletRotation);
        }
    }

    public override void Reload()
    {
        // Debug.Log($"Reloading {weaponData.WeaponName}");
    }
}
