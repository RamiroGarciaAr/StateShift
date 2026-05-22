namespace Combat.FireModes
{
    public class FullAuto : BaseFireMode
    {
        private bool _isFiring;

        public override void OnTriggerPressed() => _isFiring = true;

        public override void OnTriggerReleased() => _isFiring = false;

        public override void Tick(float deltaTime)
        {
            if (_isFiring)
                _weapon.RequestFire();
        }
    }
}
