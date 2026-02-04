namespace Core.Strategies.Health
{
    public interface IHealth
    {
        int Health { get; }
        int MaxHealth { get; }
    }
}