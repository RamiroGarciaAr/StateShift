namespace Strategies.Weapons
{
    public interface IGun
    {
        GunProperties Properties { get; }

        void Shoot();
        void Reload();
    }
}