using System;

namespace Combat.Interfaces
{
    public interface IFireMode
    {
        void Initialize(WeaponBase weapon); // * Initialize the fire mode with a reference to the weapon, this allows the fire mode to access the weapon's data and methods
        void OnTriggerPressed(); // Here we set the intent to fire
        void OnTriggerReleased();
        void Tick(float deltaTime);
    }
}
