using System.Collections;
using Flyweight.Stats;
using UnityEngine;


namespace Core.Strategies.Weapons
{
    public class Gun : MonoBehaviour, IGun
    {
        public GunProperties Properties => _properties;

        public int AmmoOnMagazine => ammoOnMagazine;
        public int AmmoOnReserve => ammoOnReserve;

        protected int ammoOnMagazine;
        protected int ammoOnReserve;

        [SerializeField] private GunProperties _properties;
        [SerializeField] private Shooter _shooter;
        [SerializeField] private MeshRenderer _meshRenderer;

        private bool _isReloading;

        private void Awake()
        {
            ammoOnMagazine = Properties.MagazineSize;
            ammoOnReserve = Properties.AmmoOnReserve;
        }

        public void Shoot()
        {
            if (_isReloading || ammoOnMagazine == 0) return;

            ammoOnMagazine--;

            _shooter.Shoot(Properties.SpreadRadius);
        }

        public void Reload()
        {
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            if (_isReloading || ammoOnMagazine == Properties.MagazineSize || ammoOnReserve == 0) yield break;
            _isReloading = true;

            yield return new WaitForSeconds(Properties.ReloadTime);

            int magazineSpace = Properties.MagazineSize - ammoOnMagazine;
            int ammoToReload = Mathf.Min(magazineSpace, ammoOnReserve);

            ammoOnMagazine += ammoToReload;
            ammoOnReserve -= ammoToReload;

            _isReloading = false;
        }

        public void Equip()
        {
            _meshRenderer.enabled = true;
        }

        public void UnEquip()
        {
            _meshRenderer.enabled = false;
        }
    }
}