using UnityEngine;

public class IKFootStepper : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField]
    private Transform _body; // the moving root

    [SerializeField]
    private Transform _footTarget; // WORLD-SPACE target (not a child of _body)

    [Header("Home anchor (body-local)")]
    [
        SerializeField,
        Tooltip("Rest foot position relative to the body. Set to this leg's natural stance.")
    ]
    private Vector3 _localAnchor = new(0.5f, 0f, 0.5f);

    private float _stepThreshold = 0.4f; // anchor drift before a step

    private float _overshoot = 0.3f; // how far past the anchor, along travel

    private float _stepDuration = 0.15f;

    private float _stepHeight = 0.3f;

    [Header("Ground")]
    [SerializeField]
    private LayerMask _groundMask;

    [SerializeField]
    private float _rayHeight = 2f;

    private Vector3 _plantedPos; // fixed world position the foot rests at
    private bool _stepping;
    private float _stepT;
    private Vector3 _stepFrom;
    private Vector3 _stepTo;

    private Vector3 _lastBodyPos;
    private Vector3 _bodyVelocity; // smoothed, world space

    public bool IsStepping => _stepping;

    private void Start()
    {
        _plantedPos = AnchorWorld();
        SampleGround(ref _plantedPos);
        if (_footTarget != null)
            _footTarget.position = _plantedPos;
        _lastBodyPos = _body.position;
    }

    private void LateUpdate()
    {
        // Track body velocity (direction-agnostic — this is what makes it omnidirectional).
        float dt = Mathf.Max(Time.deltaTime, 1e-5f);
        _bodyVelocity = (_body.position - _lastBodyPos) / dt;
        _lastBodyPos = _body.position;

        if (_stepping)
        {
            TickStep();
            return;
        }

        // Hold: foot stays nailed to its fixed world position.
        if (_footTarget != null)
            _footTarget.position = _plantedPos;

        // Step when the (body-local) anchor has drifted too far from the planted foot.
        if (Vector3.Distance(_plantedPos, AnchorWorld()) > _stepThreshold)
            BeginStep();
    }

    // Public so a gait manager can force/deny a step for pair coordination (step 3).
    public bool WantsToStep =>
        !_stepping && Vector3.Distance(_plantedPos, AnchorWorld()) > _stepThreshold;

    public void BeginStep()
    {
        _stepping = true;
        _stepT = 0f;
        _stepFrom = _plantedPos;

        Vector3 anchor = AnchorWorld();

        // Travel direction from actual velocity (flattened). Zero velocity -> no overshoot.
        Vector3 travel = _bodyVelocity;
        travel.y = 0f;
        Vector3 travelDir = travel.sqrMagnitude > 1e-6f ? travel.normalized : Vector3.zero;

        // Overshoot along travel + predicted body movement during the step.
        float predicted = travel.magnitude * _stepDuration;
        _stepTo = anchor + travelDir * (_overshoot + predicted);

        SampleGround(ref _stepTo);
    }

    private void TickStep()
    {
        _stepT += Time.deltaTime / Mathf.Max(0.01f, _stepDuration);
        float t = Mathf.Clamp01(_stepT);

        // Ease in-out so the foot decelerates into the plant (kills the arrival bounce).
        float easedT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 flat = Vector3.Lerp(_stepFrom, _stepTo, easedT);
        flat.y += Mathf.Sin(easedT * Mathf.PI) * _stepHeight; // arc uses eased t too
        if (_footTarget != null)
            _footTarget.position = flat;

        if (t >= 1f)
        {
            _stepping = false;
            _plantedPos = _stepTo;
        }
    }

    private Vector3 AnchorWorld() => _body.TransformPoint(_localAnchor);

    private void SampleGround(ref Vector3 pos)
    {
        Vector3 from = pos + Vector3.up * _rayHeight;
        if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, _rayHeight * 2f, _groundMask))
            pos = hit.point;
    }

    public void Configure(float threshold, float overshoot, float duration, float height)
    {
        _stepThreshold = threshold;
        _overshoot = overshoot;
        _stepDuration = duration;
        _stepHeight = height;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_body == null)
            return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(AnchorWorld(), 0.1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_plantedPos, 0.08f);
        if (_stepping)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_stepTo, 0.08f);
        }
    }
#endif
}
