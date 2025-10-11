using System;
using System.Collections;
using Managers;
using UnityEngine;


namespace Strategies.Weapons
{
    public class Gun : MonoBehaviour, IGun
    {
        public GunProperties Properties => _properties;

        protected int ammoOnMagazine;
        protected int ammoOnReserve;

        [SerializeField] private GunProperties _properties;
        [SerializeField] private ObjectPoolManager _bulletsPool;

        private bool _isReloading;

        private void Start()
        {
            ammoOnMagazine = Properties.MagazineSize;
            ammoOnReserve = Properties.AmmoOnReserve;
        }

        public void Shoot()
        {
            if (_isReloading || ammoOnMagazine == 0) return;

            GameObject bullet = _bulletsPool.GetPooledObject();
            if (bullet == null) return;

            ammoOnMagazine--;

            bullet.transform.SetPositionAndRotation(transform.position, transform.rotation);
            bullet.SetActive(true);
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
            int ammoToReload = Math.Min(magazineSpace, ammoOnReserve);

            ammoOnMagazine += ammoToReload;
            ammoOnReserve -= ammoToReload;

            _isReloading = false;
        }
    }
}