using System.Collections;
using System.Collections.Generic;
using Health;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class TestDmg : MonoBehaviour
{
    [Range(1f, 100f)]
    public float dmgTestAmount = 25f;

    [SerializeField]
    private BaseHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponentInChildren<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogWarning("[TEST Damage] Could not find player health");
    }

    // Update is called once per frame
    void Update() { }
}
