using System;
using System.Collections;
using System.Collections.Generic;
using Entities.Controllers;
using Health;
using UnityEngine;

//TODO: We are going to change this system to only shoot the current weapon
public class WeaponInventory : MonoBehaviour
{
    [SerializeField]
    private List<WeaponBase> weaponList = new();

    public static event Action<string> OnWeaponChanged;
    private int _currentWeaponIndex = 0;

    private Instigator entity = Instigator.Player;

    private void Start()
    {
        EquipCurrentWeapon();
        PlayerInput.OnChangeWeapon += NextWeapon;
        PlayerInput.OnShoot += HandleShoot;
    }

    private void OnDestroy()
    {
        PlayerInput.OnChangeWeapon -= NextWeapon;
        PlayerInput.OnShoot -= HandleShoot;
    }

    private void HandleShoot(bool pressed)
    {
        if (weaponList.Count == 0)
            return;
        WeaponBase current = weaponList[_currentWeaponIndex];

        if (pressed)
            current.OnTriggerPressed();
        else
            current.OnTriggerReleased();
    }

    public void NextWeapon()
    {
        if (weaponList.Count == 0)
            return;
        UnequipCurrentWeapon();
        _currentWeaponIndex = (_currentWeaponIndex + 1) % weaponList.Count;
        EquipCurrentWeapon();
    }

    private void EquipCurrentWeapon()
    {
        if (weaponList.Count == 0)
            return;

        WeaponBase weapon = weaponList[_currentWeaponIndex];
        OnWeaponChanged?.Invoke(weapon.GetWeaponName());
        weapon.gameObject.SetActive(true);
        weapon.SetOwner(entity);
        weapon.Equip();
    }

    private void UnequipCurrentWeapon()
    {
        if (weaponList.Count == 0)
            return;

        WeaponBase weapon = weaponList[_currentWeaponIndex];
        weapon.Unequip();
        weapon.gameObject.SetActive(false);
    }
}
