namespace Health
{
    public interface IHealable
    {
        void Heal(float amount);

        bool CanHeal {get;}
    }
}