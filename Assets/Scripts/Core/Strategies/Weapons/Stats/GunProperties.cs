using UnityEngine;

namespace Strategies.Weapons
{
    [CreateAssetMenu(fileName = "GunStats", menuName = "ScriptableObjects/GunStats")]
    public class GunProperties : ScriptableObject
    {
        [Header("Core Stats")]

        [SerializeField] private float fireRate = 1f;
        public float FireRate => fireRate;

        [SerializeField] private int magazineSize = 10;
        public int MagazineSize => magazineSize;

        [SerializeField] private int ammoOnReserve = 50;
        public int AmmoOnReserve => ammoOnReserve;

        [SerializeField] private float reloadTime = 2f;
        public float ReloadTime => reloadTime;
    }
}