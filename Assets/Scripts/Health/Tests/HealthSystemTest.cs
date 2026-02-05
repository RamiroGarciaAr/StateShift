using UnityEngine;
using Health;

/// <summary>
/// Test script for the Health System. Add to any GameObject to test.
/// Use the context menu options to trigger damage and healing.
/// </summary>
public class HealthSystemTest : MonoBehaviour
{
    [Header("Test References")]
    [SerializeField] private DamageMatrixSO damageMatrix;
    [SerializeField] private EnemyHealthConfigSO enemyConfig;
    [SerializeField] private PlayerHealthConfigSO playerConfig;

    [Header("Test Settings")]
    [SerializeField] private float testDamageAmount = 50f;
    [SerializeField] private DamageType testDamageType = DamageType.Kinetic;

    private EnemyHealth _testEnemy;
    private PlayerHealth _testPlayer;

    [ContextMenu("1. Test Damage Matrix")]
    public void TestDamageMatrix()
    {
        if (damageMatrix == null)
        {
            Debug.LogError("Assign a DamageMatrix SO first!");
            return;
        }

        Debug.Log("=== Damage Matrix Test ===");

        foreach (HealthType health in System.Enum.GetValues(typeof(HealthType)))
        {
            Debug.Log($"\n--- {health} ---");
            foreach (DamageType damage in System.Enum.GetValues(typeof(DamageType)))
            {
                float mult = damageMatrix.GetMultiplier(damage, health);
                string status = mult > 1f ? "<color=red>WEAK</color>" :
                               mult < 1f ? "<color=green>RESIST</color>" : "NORMAL";
                Debug.Log($"  {damage}: {mult}x ({status})");
            }
        }

        // Assertions
        Debug.Log("\n=== Assertions ===");
        AssertMultiplier(DamageType.Fire, HealthType.Flesh, 1.5f, "Flesh weak to Fire");
        AssertMultiplier(DamageType.Energy, HealthType.Exo, 1.5f, "Exo weak to Energy");
        AssertMultiplier(DamageType.Plasma, HealthType.Shield, 2f, "Shield weak to Plasma");
        AssertMultiplier(DamageType.Kinetic, HealthType.Player, 1f, "Player neutral to Kinetic");
    }

    private void AssertMultiplier(DamageType damage, HealthType health, float expected, string desc)
    {
        float actual = damageMatrix.GetMultiplier(damage, health);
        bool pass = Mathf.Approximately(actual, expected);
        string result = pass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
        Debug.Log($"{result}: {desc} (expected {expected}, got {actual})");
    }

    [ContextMenu("2. Create Test Enemy")]
    public void CreateTestEnemy()
    {
        if (enemyConfig == null || damageMatrix == null)
        {
            Debug.LogError("Assign EnemyHealthConfig and DamageMatrix first!");
            return;
        }

        // Create test enemy GameObject
        var enemyGO = new GameObject("TestEnemy");
        _testEnemy = enemyGO.AddComponent<EnemyHealth>();

        // Use reflection to set serialized fields (for testing only)
        var configField = typeof(EnemyHealth).GetField("healthConfig",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(_testEnemy, enemyConfig);

        var matrixField = typeof(BaseHealth).GetField("damageMatrix",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matrixField?.SetValue(_testEnemy, damageMatrix);

        // Re-initialize
        _testEnemy.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

        // Subscribe to events
        _testEnemy.OnHealthChanged += args =>
            Debug.Log($"[Enemy] Health: {args.CurrentHealth:F1}/{args.MaxHealth:F1} (took {args.DamageDealt:F1} damage)");
        _testEnemy.OnChunkDepleted += idx =>
            Debug.Log($"[Enemy] <color=orange>Chunk {idx} depleted!</color>");
        _testEnemy.OnDeath += () =>
            Debug.Log("[Enemy] <color=red>DIED!</color>");

        Debug.Log($"Created test enemy with {_testEnemy.MaxHealth} HP across {_testEnemy.Chunks.Count} chunks");
        foreach (var chunk in _testEnemy.Chunks)
        {
            Debug.Log($"  - {chunk.HealthType}: {chunk.MaxHealth} HP");
        }
    }

    [ContextMenu("3. Damage Test Enemy")]
    public void DamageTestEnemy()
    {
        if (_testEnemy == null)
        {
            Debug.LogError("Create test enemy first!");
            return;
        }

        var damageInfo = new DamageInfo(testDamageAmount, testDamageType);
        Debug.Log($"Dealing {testDamageAmount} {testDamageType} damage...");
        _testEnemy.TakeDamage(damageInfo);
    }

    [ContextMenu("4. Create Test Player")]
    public void CreateTestPlayer()
    {
        if (playerConfig == null || damageMatrix == null)
        {
            Debug.LogError("Assign PlayerHealthConfig and DamageMatrix first!");
            return;
        }

        var playerGO = new GameObject("TestPlayer");
        _testPlayer = playerGO.AddComponent<PlayerHealth>();

        // Use reflection to set serialized fields
        var configField = typeof(PlayerHealth).GetField("healthConfig",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(_testPlayer, playerConfig);

        var matrixField = typeof(BaseHealth).GetField("damageMatrix",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matrixField?.SetValue(_testPlayer, damageMatrix);

        // Re-initialize
        _testPlayer.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

        // Subscribe to events
        _testPlayer.OnHealthChanged += args =>
            Debug.Log($"[Player] Health: {args.CurrentHealth:F1}/{args.MaxHealth:F1} | Main: {_testPlayer.MainChunkHealth01:P0} | Side Chunks: {_testPlayer.ActiveSideChunks}/{_testPlayer.TotalSideChunks}");
        _testPlayer.OnChunkDepleted += idx =>
            Debug.Log($"[Player] <color=orange>Chunk {idx} depleted!</color>");
        _testPlayer.OnDeath += () =>
            Debug.Log("[Player] <color=red>DIED!</color>");
        _testPlayer.OnSideChunkRestored += idx =>
            Debug.Log($"[Player] <color=cyan>Side chunk {idx} restored!</color>");

        Debug.Log($"Created test player: Main {playerConfig.MainChunkHealth} HP + {playerConfig.SideChunkCount}x {playerConfig.SideChunkHealth} HP side chunks");
    }

    [ContextMenu("5. Damage Test Player")]
    public void DamageTestPlayer()
    {
        if (_testPlayer == null)
        {
            Debug.LogError("Create test player first!");
            return;
        }

        var damageInfo = new DamageInfo(testDamageAmount, testDamageType);
        Debug.Log($"Dealing {testDamageAmount} {testDamageType} damage to player...");
        _testPlayer.TakeDamage(damageInfo);
    }

    [ContextMenu("6. Restore Player Side Chunk")]
    public void RestorePlayerSideChunk()
    {
        if (_testPlayer == null)
        {
            Debug.LogError("Create test player first!");
            return;
        }

        if (_testPlayer.CanRestoreSideChunk())
        {
            _testPlayer.RestoreSideChunk();
        }
        else
        {
            Debug.Log("No side chunks to restore!");
        }
    }

    [ContextMenu("7. Cleanup Test Objects")]
    public void CleanupTestObjects()
    {
        if (_testEnemy != null) DestroyImmediate(_testEnemy.gameObject);
        if (_testPlayer != null) DestroyImmediate(_testPlayer.gameObject);
        _testEnemy = null;
        _testPlayer = null;
        Debug.Log("Test objects cleaned up.");
    }
}
