namespace Combat.FireModes
{
    public class FullAuto : BaseFireMode
    {
        public override void OnTriggerPressed()
        {
            _weapon.Shoot();
        }
    }
}