using Health;

public interface IDamagable
{
    bool IsAlive { get; }
    void TakeDamage(DamageInfo damageInfo);
}