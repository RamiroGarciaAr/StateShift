using Health;

public interface IDamagable 
{
    bool isAlive{get;}
    void TakeDamage(DamageInfo damageInfo);    
}
