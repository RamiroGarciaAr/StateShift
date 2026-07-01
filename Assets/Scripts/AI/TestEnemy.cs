using UnityEngine;
using UnityEngine.AI;

public class TestEnemy : MonoBehaviour, ITickable
{
    public bool IsTickable => _isAlive;
    private bool _isAlive = true;
    private NavMeshAgent _agent;
    private bool _destinationSet = false; 
    private Vector3 _lastDestination;   

    private const float ARRIVAL_THRESHOLD = 0.5f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        Debug.Assert(_agent != null,
            $"[TestEnemy] NavMeshAgent missing on {gameObject.name}");

        if (AITickManager.Instance == null)
        {
            Debug.LogError($"[TestEnemy] AITickManager not found.");
            return;
        }

        AITickManager.Instance.RegisterAgent(this);
    }

    private void OnDestroy()
    {
        AITickManager.Instance?.UnregisterAgent(this);
    }

    public void OnTick(float deltaTime)
    {
        if (!WaypointController.HasDestination) return;

        // Only update destination if it actually changed
        if (WaypointController.CurrentDestination != _lastDestination)
        {
            _lastDestination = WaypointController.CurrentDestination;
            _agent.SetDestination(_lastDestination);
            _destinationSet = true;
        }

        // Don't check arrival until destination is set
        if (!_destinationSet) return;

        if (HasArrived())
        {
            // Only log once on arrival not every tick
            if (_agent.isStopped == false)
            {
                _agent.isStopped = true;
                // Debug.Log($"[TestEnemy] {gameObject.name} arrived.");
            }
            return;
        }

        // Resume if stopped and new destination incoming
        if (_agent.isStopped)
            _agent.isStopped = false;
    }

    private bool HasArrived()
    {
        if (!_agent.isOnNavMesh) return false;
        if (_agent.pathPending) return false;
        if (_agent.remainingDistance == 0f && !_agent.hasPath) return false;

        return _agent.remainingDistance <= ARRIVAL_THRESHOLD;
    }
}