using System.Collections;
using Managers;
using UnityEngine;


namespace Strategies.Weapons
{
    public class Gun : MonoBehaviour, IGun
    {
        public GunProperties Properties => _properties;

        public int AmmoOnMagazine => ammoOnMagazine;
        public int AmmoOnReserve => ammoOnReserve;

        public event System.Action OnShot;
        public event System.Action OnReloaded;

        protected int ammoOnMagazine;
        protected int ammoOnReserve;

        [SerializeField] private GunProperties _properties;
        [SerializeField] private ObjectPoolManager _bulletsPool;

        private bool _isReloading;

        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            ammoOnMagazine = Properties.MagazineSize;
            ammoOnReserve = Properties.AmmoOnReserve;

            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Shoot()
        {
            if (_isReloading || ammoOnMagazine == 0) return;

            ammoOnMagazine--;

            FireBullet();

            OnShot?.Invoke();
        }

        protected virtual void FireBullet()
        {
            GameObject bullet = _bulletsPool.GetPooledObject();

            Vector3 spread = Random.insideUnitSphere * Properties.SpreadRadius;
            Vector3 direction = (transform.forward + spread).normalized;

            bullet.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(direction));
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
            int ammoToReload = Mathf.Min(magazineSpace, ammoOnReserve);

            ammoOnMagazine += ammoToReload;
            ammoOnReserve -= ammoToReload;

            _isReloading = false;

            OnReloaded?.Invoke();
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