using System.Collections;
using System.Collections.Generic;
using Entities.Controllers;
using UnityEngine;


public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private List<WeaponBase> weapons = new();
    private int currentWeaponIndex = 0;

    private void Start()
    {
        EquipCurrentWeapon();
        PlayerInput.OnChangeWeapon += NextWeapon;
    }


    public void NextWeapon()
    {
        UnequipCurrentWeapon();
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;
        EquipCurrentWeapon();
    }

    private void EquipCurrentWeapon()
    {
        if (weapons.Count == 0) return;

        WeaponBase weapon = weapons[currentWeaponIndex];
        weapon.gameObject.SetActive(true);
        weapon.Equip();
    }

    private void UnequipCurrentWeapon()
    {
        if (weapons.Count == 0) return;

        WeaponBase weapon = weapons[currentWeaponIndex];
        weapon.Unequip();
        weapon.gameObject.SetActive(false);
    }

    

}
