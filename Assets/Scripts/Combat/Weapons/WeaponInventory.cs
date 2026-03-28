using System;
using System.Collections;
using System.Collections.Generic;
using Entities.Controllers;
using UnityEngine;

//TODO: We are going to change this system to only shoot the current weapon
public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private List<WeaponBase> weaponList = new(); //TODO: Maybe change for a set
 
    public static event Action<string> OnWeaponChanged;
    private int currentWeaponIndex = 0;

    private void Start()
    {
        EquipCurrentWeapon();
        PlayerInput.OnChangeWeapon += NextWeapon;
        PlayerInput.OnShoot += ShootCurrentWeapon;

    }
    //TODO: Check if this is necesary
    private void OnDestroy()
    {
        PlayerInput.OnChangeWeapon -= NextWeapon;
        PlayerInput.OnShoot -= ShootCurrentWeapon;
    }


    public void ShootCurrentWeapon(bool isHeld = false)
    {
        if (weaponList.Count == 0) return;

        WeaponBase weapon = weaponList[currentWeaponIndex];
        //MY IDEA
        //weapon.Shoot();
        Debug.Log(weapon.GetWeaponName() + "isHeld: " + isHeld);
    }

    public void NextWeapon()
    {
        UnequipCurrentWeapon();
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponList.Count;
        EquipCurrentWeapon();
    }

    private void EquipCurrentWeapon()
    {
        if (weaponList.Count == 0) return;

        WeaponBase weapon = weaponList[currentWeaponIndex];
        OnWeaponChanged?.Invoke(weapon.GetWeaponName());
        weapon.gameObject.SetActive(true);
        weapon.Equip();
    }

    private void UnequipCurrentWeapon()
    {
        if (weaponList.Count == 0) return;

        WeaponBase weapon = weaponList[currentWeaponIndex];
        weapon.Unequip();
        weapon.gameObject.SetActive(false);
    }

    

}
