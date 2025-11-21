using UnityEngine;

namespace Flyweight.Stats
{
    [CreateAssetMenu(fileName = "GunStats", menuName = "ScriptableObjects/GunStats")]
    public class GunProperties : ScriptableObject
    {
        [SerializeField] private string gunName = "Gun";
        public string GunName => gunName;

        [Header("Core Stats")]

        [SerializeField] private float fireRate = 1f;
        public float FireRate => fireRate;

        [SerializeField] private int magazineSize = 10;
        public int MagazineSize => magazineSize;

        [SerializeField] private int ammoOnReserve = 50;
        public int AmmoOnReserve => ammoOnReserve;

        [SerializeField] private float reloadTime = 2f;
        public float ReloadTime => reloadTime;

        [SerializeField, Range(0f, 0.5f)] private float spreadRadius = 0f;
        public float SpreadRadius => spreadRadius;
    }
}