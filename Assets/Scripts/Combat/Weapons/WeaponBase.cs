using UnityEngine;
using Entities.Controllers;
using System;


public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO weaponData;
    [SerializeField] protected Transform muzzlePos;

    public static event Action<int,int> OnAmmoChanged; // * Ammo count after shot, can be used to update UI (current ammo on magazine, current ammo on reserves)

    private int currentAmmoOnMagazine;
    private int currentAmmoOnReserves;

    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }
    private void Start()
    {
        currentAmmoOnMagazine = weaponData.MagazineSize;
        currentAmmoOnReserves = weaponData.TotalAmmo; // TODO: For now we are hard coding this but then we will need to change this to be based on the player's inventory or something like that
        OnAmmoChanged?.Invoke(currentAmmoOnMagazine, currentAmmoOnReserves); // * When we initialize the weapon we want to update the UI with the current ammo count on the magazine

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
    public virtual void Equip() =>  OnAmmoChanged?.Invoke(currentAmmoOnMagazine, currentAmmoOnReserves); // * When we equip the weapon we want to update the UI with the current ammo count on the magazine
    public virtual void Unequip(){}

    public virtual void TryShoot()
    {
        // This method can be used to check for conditions before shooting, such as ammo count, fire rate, etc.
        if (currentAmmoOnMagazine > 0)
        {
            Shoot();
            currentAmmoOnMagazine--;
            OnAmmoChanged?.Invoke(currentAmmoOnMagazine, currentAmmoOnReserves);
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }
    //TODO: We need to expand this system
    public virtual void TryReload()
    {
        Debug.Log("Reloading...");
        currentAmmoOnMagazine = weaponData.MagazineSize;
        currentAmmoOnReserves -= weaponData.MagazineSize; // * This is a very simple way to handle reloading, we will need to change this to be based on the player's inventory or something like that
        if (currentAmmoOnReserves < 0) currentAmmoOnReserves = 0; // * This is to prevent the ammo count from going negative, we will need to change this to be based on the player's inventory or something like that
        OnAmmoChanged?.Invoke(currentAmmoOnMagazine, currentAmmoOnReserves);
    }

    public abstract void Reload();

    public abstract void Shoot();

}
