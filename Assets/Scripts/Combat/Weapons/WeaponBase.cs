using UnityEngine;
using Entities.Controllers;
using System;


public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO weaponData;
    [SerializeField] protected Transform muzzlePos;

    //TODO: Change the OnWeaponShot name...its confusing...
    // * IDEA: We can have OnAmmoChanged event that gets called whenever the ammo count changes, this way we can use it for both shooting and reloading, and we can pass the current
    public static event Action<int> OnWeaponShot; // * Ammo count after shot, can be used to update UI
    public static event Action<int> OnWeaponReloaded; // * We pass the new ammo left on the reserves after reload, can be used to update UI

    private int currentAmmoOnMagazine;

    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }
    private void Start()
    {
        currentAmmoOnMagazine = weaponData.MagazineSize;
        OnWeaponShot?.Invoke(currentAmmoOnMagazine); // * When we initialize the weapon we want to update the UI with the current ammo count on the magazine

    }
    void OnEnable()
    {
        PlayerInput.OnShoot += TryShoot;
        PlayerInput.OnReload += TryReload;
    }

    void OnDisable()
    {
        PlayerInput.OnShoot -= TryShoot;
        PlayerInput.OnReload -= TryReload;

    }
    public string GetWeaponName() => weaponData.WeaponName;
    public virtual void Equip() =>  OnWeaponShot?.Invoke(currentAmmoOnMagazine); // * When we equip the weapon we want to update the UI with the current ammo count on the magazine
    public virtual void Unequip(){}

    public virtual void TryShoot()
    {
        // This method can be used to check for conditions before shooting, such as ammo count, fire rate, etc.
        if (currentAmmoOnMagazine > 0)
        {
            Shoot();
            currentAmmoOnMagazine--;
            OnWeaponShot?.Invoke(currentAmmoOnMagazine);
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }
    public virtual void TryReload()
    {
        Debug.Log("Reloading...");
        currentAmmoOnMagazine = weaponData.MagazineSize;
    }

    public abstract void Reload();

    public abstract void Shoot();

}
