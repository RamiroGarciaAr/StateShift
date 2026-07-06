using UnityEngine;

namespace Health
{
    public class DamageInfo
    {
        public float BaseDamage { get; }
        public float FinalDamage { get; set; }
        public DamageType DamageType { get; }
        public Vector3 HitPoint { get; }
        public Instigator Instigator { get; set; } = Instigator.None;
        public BodyPart BodyPart { get; set; } = BodyPart.None;
        public Vector3 HitDirection { get; set; } = Vector3.zero;
        public Vector3 HitNormal { get; set; } = Vector3.zero;

        public DamageInfo(
            float baseDamage,
            DamageType damageType,
            Vector3 hitPoint = default,
            Instigator instigator = Instigator.None,
            BodyPart bodyPart = BodyPart.None,
            Vector3 hitDirection = default,
            Vector3 hitNormal = default
        )
        {
            BaseDamage = baseDamage;
            FinalDamage = baseDamage;
            DamageType = damageType;
            HitPoint = hitPoint;
            Instigator = instigator;
            BodyPart = bodyPart;
            HitDirection = hitDirection;
            HitNormal = hitNormal;

            if (Instigator == Instigator.None)
                Debug.LogError("[Damage Info]There is an Instigator being set to NONE");
        }
    }
}
