namespace Health
{
    public interface IDamageModifier 
    {
        float ModifyDamage(float baseDamage, DamageInfo damageInfo);
        int Priority {get;}
    }
}

