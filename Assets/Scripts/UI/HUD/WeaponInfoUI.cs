using Managers;
using Strategies.Weapons;
using TMPro;
using UnityEngine;

namespace UI.HUD
{
    public class WeaponInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ammoMagazineText;
        [SerializeField] private TextMeshProUGUI _ammoReserveText;
        [SerializeField] private TextMeshProUGUI _weaponNameText;

        void Start()
        {
            WeaponManager.Instance.OnEquipped += OnEquipped;
            WeaponManager.Instance.OnShot += OnShot;
            WeaponManager.Instance.OnReloaded += OnReloaded;

            OnEquipped(WeaponManager.Instance.EquippedGun);
        }

        private void OnEquipped(IGun gun)
        {
            _ammoMagazineText.text = gun.AmmoOnMagazine.ToString("00");
            _ammoReserveText.text = gun.AmmoOnReserve.ToString("00");

            _weaponNameText.text = gun.Properties.GunName;
        }

        private void OnShot(IGun gun)
        {
            _ammoMagazineText.text = gun.AmmoOnMagazine.ToString("00");
        }

        private void OnReloaded(IGun gun)
        {
            _ammoMagazineText.text = gun.AmmoOnMagazine.ToString("00");
            _ammoReserveText.text = gun.AmmoOnReserve.ToString("00");
        }
    }
}