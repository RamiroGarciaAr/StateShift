using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AINavMove : MonoBehaviour
{
    [SerializeField]
    private AIAiming _aiming;

    [SerializeField, Tooltip("Max yaw error (deg) before the bot is allowed to move")]
    private float _moveAlignAngle = 30f;

    [SerializeField]
    private float _arriveRadius = 0.5f;

    private NavMeshAgent _agent;
    private bool _hasDestination;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updatePosition = true;

        // Snap onto the navmesh at startup — guards against a bot placed slightly off the surface.
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            _agent.Warp(hit.position);
        else
            Debug.LogError(
                $"[AINavMove] No navmesh within 2m of {name} — check placement/bake",
                this
            );

        if (_aiming == null)
        {
            Debug.LogError($"[AINavMove] No AIAiming on {name}", this);
            enabled = false;
        }
    }

    public void SetDestination(Vector3 pos)
    {
        _agent.SetDestination(pos);
        _hasDestination = true;
    }

    public void Stop()
    {
        _hasDestination = false;
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
    }

    // Called from the brain's Update (motion lives in Update, not the tick)
    public void Tick()
    {
        if (!_hasDestination)
            return;

        // Feed the NEXT path corner to aiming as the facing target.
        Vector3 nextCorner = _agent.steeringTarget;
        _aiming.SetTarget(nextCorner);

        // Turn-then-move: only travel once roughly facing the corner.
        Vector3 toCorner = nextCorner - transform.position;
        toCorner.y = 0f;
        float yawError =
            toCorner.sqrMagnitude > 0.001f ? Vector3.Angle(transform.forward, toCorner) : 0f;

        _agent.isStopped = yawError > _moveAlignAngle; // crab-walk guard

        if (Arrived())
            Stop();
    }

    private bool Arrived()
    {
        if (_agent.pathPending)
            return false; // path not computed yet
        return _agent.remainingDistance <= _arriveRadius;
    }

    public bool HasArrived => _hasDestination == false; // for the brain to poll
}
