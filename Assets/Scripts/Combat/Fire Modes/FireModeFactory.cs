using Combat.Interfaces;

namespace Combat.FireModes
{
    public static class FireModeFactory
    {
        public static IFireMode Create(FireModeType type, WeaponBase weaponBase)
        {
            IFireMode mode = type switch
            {
                FireModeType.SemiAuto => new SemiAuto(),
                FireModeType.Burst => new Burst(),
                FireModeType.FullAuto => new FullAuto(),
                _ => throw new System.ArgumentException(
                    "$[FireModeFactory] Unhandled FireModeType: {type}"
                ),
            };

            mode.Initialize(weaponBase);

            return mode;
        }
    }
}
