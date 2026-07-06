using Combat.VFX;
using UnityEngine;

public class BulletTestSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private WeaponDataSO weaponData;

    [Header("VFX")]
    [Tooltip("Pooled impact effect spawner injected into spawned projectiles.")]
    [SerializeField]
    private ImpactEffectSpawner _impactSpawner;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            //bullet.GetComponent<ProjectileBase>().Initialize(weaponData, _impactSpawner);
        }
    }
}
