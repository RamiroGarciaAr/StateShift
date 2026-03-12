using UnityEngine;

namespace Health
{
    public class DamageInfo
    {
        public float BaseDamage {get;}
        public float FinalDamage {get; set;}
        public DamageType DamageType {get;}
        
        public Vector3 HitPoint {get;}

        public DamageInfo(float baseDamage, DamageType damageType, Vector3 hitPoint = default)
        {
            BaseDamage = baseDamage;
            FinalDamage = baseDamage;
            DamageType = damageType;
            HitPoint = hitPoint;
        }
    }

}
