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
            playerHealth.TakeDamage(
                new DamageInfo(dmgTestAmount, DamageType.Kinetic, default, Instigator.Enemy)
            );
            Debug.Log(
                $"Amount Dmg: {dmgTestAmount} Health Remaining: {playerHealth.CurrentHealth}"
            );
        }
    }
}
