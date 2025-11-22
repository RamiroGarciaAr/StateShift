using Managers;
using Core.Strategies.Weapons;
using TMPro;
using UnityEngine;

namespace UI.HUD
{
    public class WeaponInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _ammoMagazineText;
        [SerializeField] private TextMeshProUGUI _ammoReserveText;
        [SerializeField] private TextMeshProUGUI _weaponNameText;

        private void Update()
        {
            IGun gun = GameManager.Player.Equipment.EquippedGun;

            _ammoMagazineText.text = gun.AmmoOnMagazine.ToString("00");
            _ammoReserveText.text = gun.AmmoOnReserve.ToString("00");

            _weaponNameText.text = gun.Properties.GunName;
        }
    }
}