using UnityEngine;
using Entities.Controllers;
using Unity.VisualScripting;


public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO weaponData;
    [SerializeField] protected Transform muzzlePos;

    private int currentAmmo;


    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }
    void OnEnable()
    {
        PlayerInput.OnShoot += TryShoot;
    }

    void OnDisable()
    {
        PlayerInput.OnShoot -= TryShoot;
    }

    public virtual void Equip()
    {
        Debug.Log($"Equipping {weaponData.WeaponName}");
    }
    public virtual void Unequip()
    {
        Debug.Log($"Unequipping {weaponData.WeaponName}");
    }

    public virtual void TryShoot()
    {
        // This method can be used to check for conditions before shooting, such as ammo count, fire rate, etc.
        if (currentAmmo > 0)
        {
            Shoot();
            currentAmmo--;
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }
    public abstract void Reload();

    public abstract void Shoot();

}
