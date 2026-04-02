using UnityEngine;
using System;

public abstract class WeaponBase : MonoBehaviour, IEquipable
{

#region Fields
    [SerializeField] protected WeaponDataSO weaponData;
    [SerializeField] protected Transform muzzlePos;
    
    // ** Events to communicate with other systems (like UI, audio, etc.)
    /// <summary>
    /// Ammo Changed: This event is triggered whenever there is a change in the ammo count, whether it's shooting or reloading. It can be used to update the ammo count on the UI.
    /// - Parameters: Current ammo on magazine, current ammo on reserves
    /// Apply Bloom: This event is triggered whenever the weapon is fired and can be used to trigger the bloom effect on the crosshair. This allows us to decouple the shooting logic from the UI logic, making our code more modular and easier to maintain.
    /// </summary>
    //TODO: We might want to change this system removing the static if we want different weapons on screen at the same time, but for now we will keep it simple and have a single weapon on screen at a time
    public static event Action<int, int> OnAmmoChanged; // * Ammo count after shot, can be used to update UI (current ammo on magazine, current ammo on reserves)
    public static event Action OnApplyBloom; // * Event to trigger the crosshair bloom effect, can be used to trigger the bloom effect on the crosshair
    

    /// <summary>
    /// Ammo Management:
    /// - Current Ammo on Magazine: This is the ammo that is currently loaded in the weapon and can be fired immediately.
    /// - Current Ammo on Reserves: This is the ammo that the player has in reserve and can be used to reload the weapon when the magazine is empty or when the player decides to reload.
    /// </summary>
    private int _currentAmmoOnMagazine;
    private int _currentAmmoOnReserves;
#endregion
    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }

    // TODO: Ammo and Inventory Management
#region Unity Methods
    private void Start()
    {
        _currentAmmoOnMagazine = weaponData.MagazineSize;
        _currentAmmoOnReserves = weaponData.TotalAmmo; // TODO: For now we are hard coding this but then we will need to change this to be based on the player's inventory or something like that
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves); // * When we initialize the weapon we want to update the UI with the current ammo count on the magazine
    }

#endregion
    
#region Weapon Status
    public string GetWeaponName() => weaponData.WeaponName;
    public bool HasAmmo() => _currentAmmoOnMagazine > 0;

#endregion
    
    // TODO: Expand into different reload systems
#region Weapon Actions
    public void ConsumeAmmo(int amount)
    {
        _currentAmmoOnMagazine -= amount;
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves);
    }
    public virtual void Equip() => OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves); // * When we equip the weapon we want to update the UI with the current ammo count on the magazine
    public virtual void Unequip() { }    
    //TODO: We need to expand this system to handle different reload systems
    public virtual void TryReload()
    {
        if (_currentAmmoOnMagazine >= weaponData.MagazineSize) return;

        if (_currentAmmoOnReserves <= 0) return;
        int amountNeeded = weaponData.MagazineSize - _currentAmmoOnMagazine;

        // Take what we need, or whatever is left in reserves
        int amountToTake = Mathf.Min(amountNeeded, _currentAmmoOnReserves);

        //We do this so we update a single time the Invoke
        _currentAmmoOnReserves -= amountToTake;
        _currentAmmoOnMagazine += amountToTake;

        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves);

    }
#endregion
    
    public abstract void Reload();
    public abstract void Shoot();
}
