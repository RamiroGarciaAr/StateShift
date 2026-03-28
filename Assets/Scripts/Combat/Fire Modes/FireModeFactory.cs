using Combat.FireModes;
using System;
public static class FireModeFactory
{
    public static IFireMode CreateFireMode(FireModeType fireModeType, float fireRate)
    {
        return fireModeType switch
        {
            FireModeType.SemiAuto => new SemiAutoFireMode(fireRate),
           // FireModeType.Burst => new BurstFireMode(fireRate),
            //FireModeType.FullAuto => new FullAutoFireMode(fireRate),
            _ => throw new ArgumentException($"Unsupported fire mode type: {fireModeType}")
        };
    }
}
