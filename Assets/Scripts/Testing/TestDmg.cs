using Health;
using UnityEngine;

public class TestDmg : MonoBehaviour
{
    [Range(1f, 100f)]
    public float dmgTestAmount = 25f;

    private BaseHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponentInChildren<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogWarning("[TEST Damage] Could not find player health");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && playerHealth != null)
        {
            DamageInfo damageInfo = new DamageInfo(
                dmgTestAmount,
                DamageType.Kinetic,
                default,
                Instigator.Enemy
            );
            playerHealth.TakeDamage(damageInfo);
            Debug.Log(
                $"[TEST Damage] Base: {damageInfo.BaseDamage}, Final: {damageInfo.FinalDamage}, Health Remaining: {playerHealth.CurrentHealth}"
            );
        }
    }
}
