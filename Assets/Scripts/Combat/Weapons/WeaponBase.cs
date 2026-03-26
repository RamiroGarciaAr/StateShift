using UnityEngine;
using Entities.Controllers;
using System;


public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO weaponData;
    [SerializeField] protected Transform muzzlePos;

    public static event Action<int, int> OnAmmoChanged; // * Ammo count after shot, can be used to update UI (current ammo on magazine, current ammo on reserves)
    public static event Action OnApplyBloom; // * Event to trigger the crosshair bloom effect, can be used to trigger the bloom effect on the crosshair
    private int _currentAmmoOnMagazine;
    private int _currentAmmoOnReserves;

    private float _shootTimer = 0f;

    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }
    private void Start()
    {
        _currentAmmoOnMagazine = weaponData.MagazineSize;
        _currentAmmoOnReserves = weaponData.TotalAmmo; // TODO: For now we are hard coding this but then we will need to change this to be based on the player's inventory or something like that
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves); // * When we initialize the weapon we want to update the UI with the current ammo count on the magazine

    }

    void Update()
    {
        if (_shootTimer > 0f)
            _shootTimer -= Time.deltaTime;

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
    public virtual void Equip() => OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves); // * When we equip the weapon we want to update the UI with the current ammo count on the magazine
    public virtual void Unequip() { }
    
    // This method can be used to check for conditions before shooting, such as ammo count, fire rate, etc.
    public virtual void TryShoot()
    {
        if (_shootTimer > 0f) return; // Check fire rate
        if (_currentAmmoOnMagazine <= 0)
        {
            Debug.Log("Out of ammo, need to reload!");
            return;
        }
        OnApplyBloom?.Invoke();
        Shoot();
        _currentAmmoOnMagazine--;
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves);
        _shootTimer = weaponData.SecondsBetweenShots;
    }
    //TODO: We need to expand this system
    public virtual void TryReload()
    {
        if (_currentAmmoOnMagazine >= weaponData.MagazineSize) return;

        if (_currentAmmoOnReserves <= 0) return;
        int amountNeeded = weaponData.MagazineSize - _currentAmmoOnMagazine;

        // Take what we need, or whatever is left in reserves
        int amountToTake = Mathf.Min(amountNeeded, _currentAmmoOnReserves);

        _currentAmmoOnReserves -= amountToTake;
        _currentAmmoOnMagazine += amountToTake;

        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves);

    }

    public abstract void Reload();

    public abstract void Shoot();

}
