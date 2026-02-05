
namespace Health
{
    public class HealthChangeEventArgs
    {
        public float PreviousHealth {get;}
        public float CurrentHealth {get;}
        public float MaxHealth {get;}
        public float DamageDealt {get;}
        public int ChunkIdx {get;}
        public bool ChunkDepleted {get;}

        public HealthChangeEventArgs(float prev, float current,float max, float damage, int chunk,bool depleted)
        {
            PreviousHealth = prev;
            CurrentHealth = current;
            MaxHealth = max;
            DamageDealt = damage;
            ChunkIdx = chunk;
            ChunkDepleted = depleted;
        }
    }
}