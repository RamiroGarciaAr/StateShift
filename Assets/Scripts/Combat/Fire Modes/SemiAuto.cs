namespace Combat.FireModes
{
    public class SemiAuto : BaseFireMode
    {
        public override void OnTriggerPressed() => _weapon.RequestFire();
    }
}
