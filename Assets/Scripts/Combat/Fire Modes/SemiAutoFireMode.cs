using Combat.FireModes;

public class SemiAutoFireMode : IFireMode
{
    private readonly float _timeBetweenShots;
    private float _timer = 0f;

    public SemiAutoFireMode(float fireRate) => _timeBetweenShots = 1f / fireRate;

    //Consumes & Checks Ammo
    //Checks Fire Rate
    //Shoots Weapon
    public void OnTriggerPressed(WeaponBase weapon)
    {
        if (_timer > 0f) return; // Check fire rate

        if (!weapon.HasAmmo()) return; // Check ammo

        weapon.Shoot(); // Shoot the weapon
        weapon.ConsumeAmmo(1); // Consume ammo

        _timer = _timeBetweenShots; // Reset the timer

    }
    
    public void OnTriggerReleased()
    {
        // Semi-auto doesn't have any special behavior on trigger release, so we do nothing here
    }


    public void Execute(float deltaTime)
    {
        if(_timer > 0f)
            _timer -= deltaTime;
    }
}
