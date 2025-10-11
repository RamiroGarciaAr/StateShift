using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "ScriptableObjects/WeaponStats")]
public class GunProperties : ScriptableObject
{
    [Header("Core Stats")]
    public string displayName = "Test Gun";

    [SerializeField] private float fireRate = 1f;
    public float FireRate => fireRate;

    [SerializeField] private int magazineSize = 10;
    public int MagazineSize => magazineSize;

    [SerializeField] private int ammoOnReserve = 50;
    public int AmmoOnReserve => ammoOnReserve;

    [SerializeField] private float reloadTime = 2f;
    public float ReloadTime => reloadTime;

    [SerializeField] private bool isAutomatic = false;
    public bool IsAutomatic => isAutomatic;
}