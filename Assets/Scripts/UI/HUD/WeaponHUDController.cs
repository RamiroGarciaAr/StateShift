using Entities.Controllers;
using TMPro;

using UnityEngine;

public class WeaponHUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text ammoOnMagazineText;
    [SerializeField] private TMP_Text ammoOnReservesText;

    private void Start()
    {
        WeaponInventory.OnWeaponChanged += UpdateWeaponName;
        WeaponBase.OnAmmoChanged += UpdateAmmoCount;

    }
    private void OnDestroy()
    {
        WeaponInventory.OnWeaponChanged -= UpdateWeaponName;
        WeaponBase.OnAmmoChanged -= UpdateAmmoCount;
    }

    private void UpdateAmmoCount(int ammoOnMagazine, int ammoOnReserves)
    {
        ammoOnMagazineText.text = ammoOnMagazine.ToString();
        ammoOnReservesText.text = ammoOnReserves.ToString();
    }
    private void UpdateWeaponName(string weaponName) => weaponNameText.text = weaponName;

}
