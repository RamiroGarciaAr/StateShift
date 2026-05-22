namespace Combat.FireModes
{
    public class Burst : BaseFireMode
    {
        private int _shotsFired;
        private bool _isBursting,
            _burstLock;

        private float _timeSinceLastShot;

        public override void OnTriggerPressed()
        {
            if (_burstLock)
                return;
            _isBursting = true;
        }

        public override void OnTriggerReleased()
        {
            _burstLock = false;
            _isBursting = false;
            _shotsFired = 0;
        }

        public override void Tick(float deltaTime)
        {
            if (!_isBursting || _shotsFired >= _weapon.BurstCount)
            {
                if (_shotsFired >= _weapon.BurstCount)
                    _burstLock = true;
                return;
            }
            _timeSinceLastShot -= deltaTime;
            if (_timeSinceLastShot <= 0 && _isBursting && _shotsFired < _weapon.BurstCount)
            {
                _weapon.RequestFire();
                _shotsFired++;
                _timeSinceLastShot = _weapon.SecondsBetweenShots;
            }
        }
    }
}
