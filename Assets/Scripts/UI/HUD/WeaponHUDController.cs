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
        WeaponBase.OnWeaponShot += UpdateAmmoCount;

    }
    private void OnDestroy()
    {
        WeaponInventory.OnWeaponChanged -= UpdateWeaponName;
        WeaponBase.OnWeaponShot -= UpdateAmmoCount;
    }

    private void UpdateAmmoCount(int ammoCount) => ammoOnMagazineText.text = ammoCount.ToString();
    private void UpdateWeaponName(string weaponName) => weaponNameText.text = weaponName;
}
