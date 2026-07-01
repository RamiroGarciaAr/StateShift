using System;
using Combat.FireModes;
using Combat.Interfaces;
using Entities.Controllers;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IEquipable
{
    #region Fields
    [SerializeField]
    protected WeaponDataSO weaponData;

    public float SecondsBetweenShots => weaponData.SecondsBetweenShots;
    public int BurstCount => weaponData.BurstCount;

    [SerializeField]
    protected Transform muzzlePos;

    // ** Events to communicate with other systems (like UI, audio, etc.)
    /// <summary>
    /// Ammo Changed: This event is triggered whenever there is a change in the ammo count, whether it's shooting or reloading. It can be used to update the ammo count on the UI.
    /// - Parameters: Current ammo on magazine, current ammo on reserves
    /// Apply Bloom: This event is triggered whenever the weapon is fired and can be used to trigger the bloom effect on the crosshair. This allows us to decouple the shooting logic from the UI logic, making our code more modular and easier to maintain.
    /// </summary>
    //TODO: We might want to change this system removing the static if we want different weapons on screen at the same time, but for now we will keep it simple and have a single weapon on screen at a time
    public static event Action<int, int> OnAmmoChanged; // * Ammo count after shot, can be used to update UI (current ammo on magazine, current ammo on reserves)
    public static event Action OnShoot; // * Event to trigger whenever the weapon is fired, this can be used to trigger the bloom effect on the crosshair or other effects that should happen when the weapon is fired

    //TODO: We might want to change this system to a more tighly coupled system where the weapon directly communicates with the UI and other systems instead of using events, but for now we will keep it simple and use events to decouple the systems
    public static event Action<float> OnReloadAnimation; // * Event to trigger whenever the weapon is reloaded, this can be used to trigger the reload animation or other effects that should happen when the weapon is reloaded

    /// <summary>
    /// Ammo Management:
    /// - Current Ammo on Magazine: This is the ammo that is currently loaded in the weapon and can be fired immediately.
    /// - Current Ammo on Reserves: This is the ammo that the player has in reserve and can be used to reload the weapon when the magazine is empty or when the player decides to reload.
    /// </summary>
    private int _currentAmmoOnMagazine;
    private int _currentAmmoOnReserves;

    private bool _wantsToFire;
    private float _fireTimer;

    private IFireMode _currentFireMode;
    private int _currentFireModeIdx;
    #endregion
    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }

    // TODO: Ammo and Inventory Management
    #region Unity Methods
    private void OnEnable()
    {
        // * We want to reset the ammo count when we enable the weapon, this is useful for when we switch weapons or when we pick up a new weapon
        _currentAmmoOnMagazine = weaponData.MagazineSize;
        _currentAmmoOnReserves = weaponData.TotalAmmo; // TODO: For now we are hard coding this but then we will need to change this to be based on the player's inventory or something like that
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves); // * When we initialize the weapon we want to update the UI with the current ammo count on the magazine
        PlayerInput.OnReload += TryReload; // * We subscribe to the reload event to handle the reload logic when the player presses the reload button
    }

    private void OnDisable()
    {
        PlayerInput.OnReload -= TryReload; // * We unsubscribe from the reload event to avoid memory leaks and to ensure that we don't try to reload a weapon that is not active
    }

    private void Start()
    {
        InitializeFireMode();
    }

    private void Update()
    {
        _fireTimer -= Time.deltaTime;
        _currentFireMode?.Tick(Time.deltaTime);
        if (_wantsToFire && _fireTimer <= 0f && HasAmmo())
        {
            Shoot();
            OnShoot?.Invoke();
            ConsumeAmmo(1); // todo: yes we are hard coding this for now but then we will need to change this to be based on the weapons data
            _fireTimer = SecondsBetweenShots;
        }
        _wantsToFire = false; // * We set this to false because we only want to shoot once per trigger press, the fire mode will handle the logic for automatic weapons or burst fire weapons
    }

    #endregion

    #region Fire Mode
    private void InitializeFireMode()
    {
        if (weaponData.AvailableFireModes == null || weaponData.AvailableFireModes.Length == 0)
        {
            Debug.LogWarning($"[Weapon Base] {weaponData.WeaponName} has no fire Modes");
            return;
        }

        _currentFireModeIdx = 0;
        SetFireMode(_currentFireModeIdx);
    }

    private void SetFireMode(int idx)
    {
        _currentFireMode = FireModeFactory.Create(weaponData.AvailableFireModes[idx], this);
    }
    #endregion


    #region Weapon Status
    public string GetWeaponName() => weaponData.WeaponName;

    public bool HasAmmo() => _currentAmmoOnMagazine > 0;

    #endregion

    // TODO: Expand into different reload systems
    #region Weapon Actions

    /*
        This is called whenever the player presses the trigger,we set the intent to fire
    */
    public void RequestFire() => _wantsToFire = true;

    public void OnTriggerPressed() => _currentFireMode?.OnTriggerPressed();

    public void OnTriggerReleased() => _currentFireMode?.OnTriggerReleased();

    public void StopFiring()
    {
        _currentFireMode?.OnTriggerReleased();
        _wantsToFire = false;
    }

    public void ConsumeAmmo(int amount)
    {
        _currentAmmoOnMagazine -= amount;
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves);
    }

    public virtual void Equip() =>
        OnAmmoChanged?.Invoke(_currentAmmoOnMagazine, _currentAmmoOnReserves); // * When we equip the weapon we want to update the UI with the current ammo count on the magazine

    public virtual void Unequip() { }

    //TODO: We need to expand this system to handle different reload systems
    public virtual void TryReload()
    {
        if (_currentAmmoOnMagazine >= weaponData.MagazineSize)
            return;

        if (_currentAmmoOnReserves <= 0)
            return;
        OnReloadAnimation?.Invoke(weaponData.ReloadTime); // * We trigger the reload animation and pass the reload time to it so it can sync the animation with the reload time
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
