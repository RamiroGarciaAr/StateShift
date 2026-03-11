using UnityEngine;

/// <summary>
/// TEMPORARY TEST SCRIPT — Delete before Alpha.
/// Validates that AITickManager correctly distributes
/// ticks across registered agents.
/// </summary>
public class TestEnemy : MonoBehaviour, ITickable
{
    // ─────────────────────────────────────────
    // ITICKABLE
    // ─────────────────────────────────────────

    public bool IsTickable => _isAlive;
    private bool _isAlive = true;

    // ─────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────

    private void Awake()
    {
        AITickManager.Instance.RegisterAgent(this);
        Debug.Log($"[TestEnemy] {gameObject.name} registered.");
    }

    private void OnDestroy()
    {
        AITickManager.Instance?.UnregisterAgent(this);
        Debug.Log($"[TestEnemy] {gameObject.name} unregistered.");
    }

    // ─────────────────────────────────────────
    // ITICKABLE IMPLEMENTATION
    // ─────────────────────────────────────────

    public void OnTick(float deltaTime)
    {
        Debug.Log($"[TestEnemy] {gameObject.name} ticked at {Time.time:F2}s | frame {Time.frameCount}"); 
    }
}