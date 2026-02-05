using System;

namespace Health
{
    public interface IHealth
    {
        float CurrentHealth {get;}
        float MaxHealth {get;}
        float HealthNormalize {get;}
        bool isAlive{get;}

        event Action<HealthChangeEventArgs> OnHealthChanged;
        event Action OnDeath;
         
    }
}