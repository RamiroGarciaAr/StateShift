using Health;
using UnityEngine;

// Temporary test component
public class DamageTestTarget : MonoBehaviour, IDamagable
{
    public bool IsAlive => true;

    public void TakeDamage(DamageInfo damageInfo)
    {
        Debug.Log($"[HIT] Damage: {damageInfo.FinalDamage:F1} | " +
                  $"Type: {damageInfo.DamageType} | " +
                  $"Point: {damageInfo.HitPoint}");
    }
}