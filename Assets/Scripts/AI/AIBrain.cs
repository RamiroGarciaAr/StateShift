using Health;
using UnityEngine;

[RequireComponent(typeof(AIMemory))]
[RequireComponent(typeof(EnemyHealth))]
public class AIBrain : MonoBehaviour, ITickable
{
    [Header("References")]
    [SerializeField]
    private AIPerception perception;

    [SerializeField]
    private AIAiming aiming;

    private AIMemory _memory;
    private EnemyHealth _health;

    public bool IsTickable => true; // reserved for stuns or culls OR maybe its a sign that we need to remove it

    public void OnTick(float dt)
    {
        _memory.Report(perception.Sample());
        if (_memory.LastSeenPlayerTime >= 0f)
            aiming.SetTarget(_memory.LastKnownPlayerPosition);
    }

    private void Awake()
    {
        _memory = GetComponent<AIMemory>();
        _health = GetComponent<EnemyHealth>();

        if (perception == null)
        {
            Debug.LogError($"[AIBrain] No Perception assigned on {gameObject.name}", this);
            enabled = false;
            return;
        }
        if (aiming == null)
        {
            Debug.LogError($"[AIBrain] No Aiming assigned on {gameObject.name}", this);
            enabled = false;
            return;
        }
        _health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    private void OnEnable()
    {
        if (AITickManager.Instance == null)
        {
            Debug.LogError("[AI Tick] No tick manager was found", this);
            return;
        }
        AITickManager.Instance.RegisterAgent(this);
    }

    private void OnDisable()
    {
        AITickManager.Instance?.UnregisterAgent(this);
    }

    private void HandleDeath()
    {
        enabled = false;
        aiming.enabled = false;
    }
}
