using UnityEngine;

public interface IGun
{
    GunProperties Properties { get; }
    GameObject BulletPrefab { get; }

    void Shoot();
    void Reload();
}