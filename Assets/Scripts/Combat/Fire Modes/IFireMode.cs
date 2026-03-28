using System;
namespace Combat.FireModes
{
    public interface IFireMode
    {
        void OnTriggerPressed(WeaponBase weapon);
        void OnTriggerReleased();
        void Execute(float deltaTime); 
    }

    public enum FireModeType
    {
        SemiAuto,
        Burst,
        FullAuto
    }
}
