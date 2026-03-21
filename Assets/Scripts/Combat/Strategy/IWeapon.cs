
public interface IWeapon
{
    void Initialize (WeaponDataSO data);

    void Equip();
    void Unequip();
    void Shoot();
    void Reload();
}
