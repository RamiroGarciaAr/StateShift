using Combat.Interfaces;

namespace Combat.FireModes
{
    public class BaseFireMode : IFireMode
    {
        protected WeaponBase _weapon;
        public void Initialize(WeaponBase weapon)
        {
           _weapon = weapon;
        }

        public virtual void OnTriggerPressed()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnTriggerReleased() {}

        public virtual void Tick(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
    }
}