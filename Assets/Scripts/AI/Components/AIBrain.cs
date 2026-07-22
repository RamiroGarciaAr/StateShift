using Health;
using Unity.AI;
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

    [SerializeField]
    private AIFiring firing;

    // new serialized refs (or GetComponent in Awake):
    [SerializeField]
    private AINavMove navMove;

    [SerializeField]
    private AIPatrol patrol;

    // new accessors, alongside Memory/Aiming/TickDelta:
    public AINavMove NavMove => navMove;
    public AIPatrol Patrol => patrol;

    [Header("Alert")]
    [SerializeField, Tooltip("Seconds of lost LOS before Alerted drops to Suspicious")]
    private float _losGraceTime = 2f;

    [SerializeField, Tooltip("Seconds in Suspicious with no reacquire before giving up to Unaware")]
    private float _searchGiveUpTime = 8f;

    private AIMemory _memory;
    private EnemyHealth _health;

    private StateMachine<AIMemory.AlertLevel> _alertMachine;
    private SuspiciousState _suspiciousState; // held instance so we can read ElapsedSearch
    private float _timeSinceLostLOS;

    // Exposed to the alert states:
    public float TickDelta { get; private set; }
    public AIMemory Memory => _memory;
    public AIAiming Aiming => aiming;

    public bool IsTickable => true; // reserved for stun/cull

    private void Awake()
    {
        _memory = GetComponent<AIMemory>();
        _health = GetComponent<EnemyHealth>();

        if (perception == null || aiming == null || firing == null)
        {
            Debug.LogError($"[AIBrain] Missing references on {name}", this);
            enabled = false;
            return;
        }
        _health.OnDeath += HandleDeath;
        _health.OnDamaged += HandleDamaged;

        _alertMachine = new StateMachine<AIMemory.AlertLevel>();
        _suspiciousState = new SuspiciousState(this);
        _alertMachine.RegisterState(AIMemory.AlertLevel.Unaware, new UnawareState(this));
        _alertMachine.RegisterState(AIMemory.AlertLevel.Suspicious, _suspiciousState);
        _alertMachine.RegisterState(AIMemory.AlertLevel.Alerted, new AlertedState(this));
        _alertMachine.Initialize(AIMemory.AlertLevel.Unaware);
    }

    private void Update()
    {
        if (navMove != null)
            navMove.Tick();
        _alertMachine?.CurrentState?.OnUpdate();
    }

    public void OnTick(float dt)
    {
        TickDelta = dt;

        _memory.Report(perception.Sample());
        firing.SetFiringState(_memory.CanSeePlayer, _memory.LastKnownPlayerPosition);

        EvaluateTransitions();
        _alertMachine.Update(); // current state acts
    }

    private void EvaluateTransitions()
    {
        AIMemory.AlertLevel level = _alertMachine.CurrentStateType;
        bool canSee = _memory.CanSeePlayer;

        switch (level)
        {
            case AIMemory.AlertLevel.Unaware:
                if (canSee)
                    _alertMachine.ChangeState(AIMemory.AlertLevel.Alerted);
                break;

            case AIMemory.AlertLevel.Alerted:
                if (canSee)
                {
                    _timeSinceLostLOS = 0f;
                }
                else
                {
                    _timeSinceLostLOS += TickDelta;
                    if (_timeSinceLostLOS >= _losGraceTime)
                        _alertMachine.ChangeState(AIMemory.AlertLevel.Suspicious);
                }
                break;

            case AIMemory.AlertLevel.Suspicious:
                if (canSee)
                    _alertMachine.ChangeState(AIMemory.AlertLevel.Alerted);
                else if (_suspiciousState.ElapsedSearch >= _searchGiveUpTime)
                    _alertMachine.ChangeState(AIMemory.AlertLevel.Unaware);
                break;
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDeath -= HandleDeath;
            _health.OnDamaged -= HandleDamaged;
        }
    }

    private void OnEnable()
    {
        if (AITickManager.Instance == null)
        {
            Debug.LogError("[AIBrain] No tick manager found", this);
            return;
        }
        AITickManager.Instance.RegisterAgent(this);
    }

    private void OnDisable()
    {
        AITickManager.Instance?.UnregisterAgent(this);
    }

    private void HandleDamaged(DamageInfo info)
    {
        if (_memory.CanSeePlayer)
            return;

        float guessDist = 8f;
        Vector3 approx = transform.position - info.HitDirection.normalized * guessDist;
        approx += Random.insideUnitSphere * 2f;
        approx.y = transform.position.y;

        if (UnityEngine.AI.NavMesh.SamplePosition(approx, out UnityEngine.AI.NavMeshHit hit, 4f, UnityEngine.AI.NavMesh.AllAreas))
            approx = hit.position;

        _memory.SeedSuspicion(approx);
        _alertMachine.ChangeState(AIMemory.AlertLevel.Suspicious);
    }

    private void HandleDeath()
    {
        enabled = false;
        aiming.enabled = false;
        firing.enabled = false;
    }
}
