namespace Combat.FireModes
{
    public class Burst : BaseFireMode
    {
        public override void OnTriggerPressed()
        {
            _weapon.Shoot();
        }
    }
}