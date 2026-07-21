using UnityEngine;

[RequireComponent(typeof(AIMemory))]
public class AIBrain : MonoBehaviour, ITickable
{
    [Header("References")]
    [SerializeField]
    private AIPerception perception;

    [SerializeField]
    private AIAiming aiming;

    [SerializeField]
    private AIFiring firing;

    private AIMemory _memory;

    public bool IsTickable => true; // reserved for stuns or culls OR maybe its a sign that we need to remove it

    public void OnTick(float dt)
    {
        _memory.Report(perception.Sample());
        if (_memory.LastSeenPlayerTime >= 0f)
            aiming.SetTarget(_memory.LastKnownPlayerPosition);
        firing.SetFiringState(_memory.CanSeePlayer, _memory.LastKnownPlayerPosition);
    }

    private void Awake()
    {
        _memory = GetComponent<AIMemory>();

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
        if (firing == null)
        {
            Debug.LogError($"[AIBrain] No Firing assigned on {gameObject.name}", this);
            enabled = false;
            return;
        }
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
}
