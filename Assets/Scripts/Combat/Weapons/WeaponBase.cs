using UnityEngine;


public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO weaponData;

    public virtual void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }

    public virtual void Equip()
    {
        Debug.Log($"Equipping {weaponData.WeaponName}");
    }
    public virtual void Unequip()
    {
        Debug.Log($"Unequipping {weaponData.WeaponName}");
    }

    public abstract void Reload();

    public abstract void Shoot();

}
