using UnityEngine;

public class BulletTestSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private WeaponDataSO weaponData;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            bullet.GetComponent<ProjectileBase>().Initialise(weaponData);
        }
    }

}
