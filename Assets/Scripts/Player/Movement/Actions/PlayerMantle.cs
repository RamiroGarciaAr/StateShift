using StateShift.Player.MantleLogic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMantle : MonoBehaviour
{
    private const float HeadroomCheckRadius = 0.25f;
    private const float MinMantleDuration = 0.01f;

    [Header("Detection")]
    [Tooltip("How far forward the player probes for a mantleable wall face.")]
    [SerializeField] private float _forwardCheckDistance = 1.1f;

    [Tooltip("Height above the feet at which the forward wall-detection ray is cast.")]
    [SerializeField] private float _forwardRayHeight = 0.9f;

    [Tooltip("Lowest ledge height (from feet to ledge top) the player is allowed to mantle.")]
    [SerializeField] private float _minLedgeHeight = 0.8f;

    [Tooltip("Highest ledge height (from feet to ledge top) the player is allowed to mantle.")]
    [SerializeField] private float _maxLedgeHeight = 1.6f;

    [Tooltip("Vertical clearance required above the ledge top for the player to fit.")]
    [SerializeField] private float _ledgeTopClearanceHeight = 1.0f;

    [Tooltip("How far past the wall face the downward ray probes to find the ledge top.")]
    [SerializeField] private float _lipProbeForward = 0.3f;

    [Tooltip("Layers considered valid mantleable geometry (walls, ledges).")]
    [SerializeField] private LayerMask _mantleLayerMask = ~0;

    [Header("Grapple Auto-Mantle")]
    [Tooltip("Forward detection reach used when auto-mantling on grapple end (grapples stop short of the wall, so this is larger than the jump reach).")]
    [SerializeField] private float _grappleMantleReach = 4f;

    [Tooltip("Maximum ledge height accepted when auto-mantling on grapple end (more permissive, since a grapple often finishes below the lip).")]
    [SerializeField] private float _grappleMaxLedgeHeight = 2.5f;

    [Header("Trajectory")]
    [Tooltip("Total duration of the scripted mantle motion, in seconds.")]
    [SerializeField] private float _mantleDuration = 0.35f;

    [Tooltip("Curve driving horizontal (XZ) progress from start to landing over the mantle.")]
    [SerializeField] private AnimationCurve _horizontalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Curve driving vertical (Y) progress from start to landing over the mantle.")]
    [SerializeField] private AnimationCurve _verticalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("How far past the ledge lip the player is planted on completion.")]
    [SerializeField] private float _landingForwardOffset = 0.4f;

    [Tooltip("Forward exit speed applied on completion so the player keeps moving out of the mantle.")]
    [SerializeField] private float _exitForwardSpeed = 3f;

    [Header("Momentum")]
    [Range(0f, 1f)]
    [Tooltip("Small momentum burst added to the player when the mantle completes.")]
    [SerializeField] private float _momentumGain = 0.15f;

    [Header("Cooldown")]
    [Tooltip("Time before another mantle can be triggered, preventing re-trigger spam.")]
    [SerializeField] private float _mantleCooldown = 0.4f;

    private Rigidbody _rb;
    private PlayerMovement _playerMovement;
    private CapsuleCollider _capsuleCollider;
    private Camera _mainCamera;

    private bool _isMantling;
    private float _timer;
    private float _cooldownTimer;
    private bool _originalGravitySetting;
    private bool _originalKinematicSetting;
    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _mantleFacing;
    private RaycastHit _hit;

    #region Properties
    /// <summary>True once the cooldown has elapsed and no mantle is currently running.</summary>
    public bool CanMantle => _cooldownTimer <= 0f && !_isMantling;

    /// <summary>True while the scripted mantle trajectory is being driven. Polled by MantlingState.</summary>
    public bool IsMantling => _isMantling;

    /// <summary>Normalized progress [0,1] through the current mantle, or 0 when not mantling. Used by camera tilt.</summary>
    public float MantleProgress01 => _isMantling ? Mathf.Clamp01(_timer / _mantleDuration) : 0f;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _mainCamera = Camera.main;

        _mantleDuration = Mathf.Max(MinMantleDuration, _mantleDuration);
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (_isMantling)
        {
            UpdateMantle();
        }
    }

    /// <summary>
    /// Pure detection query with no side effects. Returns true when a valid, in-range ledge is
    /// currently in front of the player. Used by the grounded/in-air states for jump-mantle.
    /// </summary>
    public bool HasMantleableLedge()
    {
        return DetectLedge(_forwardCheckDistance, _maxLedgeHeight, out _, out _, out _);
    }

    /// <summary>
    /// Attempts to begin a jump-triggered mantle. Runs detection and, if a valid ledge is found and
    /// the action is off cooldown, snapshots the trajectory and starts the scripted motion.
    /// </summary>
    public bool TryStartMantle()
    {
        return TryStartMantleInternal(_forwardCheckDistance, _maxLedgeHeight);
    }

    /// <summary>
    /// Pure detection query using the more permissive grapple reach/height window. No side effects.
    /// Used by GrapplingState to auto-mantle when a grapple ends near a low ledge.
    /// </summary>
    public bool HasMantleableLedgeFromGrapple()
    {
        return DetectLedge(_grappleMantleReach, _grappleMaxLedgeHeight, out _, out _, out _);
    }

    /// <summary>
    /// Attempts to begin an automatic mantle on grapple end, using the larger grapple reach and
    /// height window. Returns true if a valid ledge was found and the mantle started.
    /// </summary>
    public bool TryStartMantleFromGrapple()
    {
        return TryStartMantleInternal(_grappleMantleReach, _grappleMaxLedgeHeight);
    }

    private bool TryStartMantleInternal(float forwardCheckDistance, float maxLedgeHeight)
    {
        if (!CanMantle)
        {
            return false;
        }

        if (!DetectLedge(forwardCheckDistance, maxLedgeHeight, out Vector3 startPos, out Vector3 landingPos, out Vector3 facing))
        {
            return false;
        }

        StartMantle(startPos, landingPos, facing);
        return true;
    }

    /// <summary>Aborts an in-progress mantle and restores the saved gravity setting. Used for interrupts.</summary>
    public void CancelMantle()
    {
        if (!_isMantling)
        {
            return;
        }

        _isMantling = false;
        _timer = 0f;
        _rb.isKinematic = _originalKinematicSetting;
        _rb.useGravity = _originalGravitySetting;
        _cooldownTimer = _mantleCooldown;
    }

    private bool DetectLedge(float forwardCheckDistance, float maxLedgeHeight, out Vector3 startPos, out Vector3 landingPos, out Vector3 facing)
    {
        startPos = _rb.position;
        landingPos = _rb.position;
        facing = transform.forward;

        if (_capsuleCollider == null || _mainCamera == null)
        {
            return false;
        }

        facing = GetFacingDirection();
        float feetY = _capsuleCollider.bounds.min.y;

        Vector3 forwardOrigin = new Vector3(_rb.position.x, feetY + _forwardRayHeight, _rb.position.z);
        Ray forwardRay = new Ray(forwardOrigin, facing);
        if (!Physics.Raycast(forwardRay, out _hit, forwardCheckDistance, _mantleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        Vector3 wallHit = _hit.point;

        float downRayHeight = feetY + maxLedgeHeight + _ledgeTopClearanceHeight;
        Vector3 downOrigin = new Vector3(wallHit.x, downRayHeight, wallHit.z) + facing * _lipProbeForward;
        Ray downRay = new Ray(downOrigin, Vector3.down);
        float downDistance = maxLedgeHeight + _ledgeTopClearanceHeight;
        if (!Physics.Raycast(downRay, out _hit, downDistance, _mantleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        Vector3 ledgeTop = _hit.point;
        float ledgeHeight = ledgeTop.y - feetY;
        if (!MantleMath.IsLedgeHeightValid(ledgeHeight, _minLedgeHeight, maxLedgeHeight))
        {
            return false;
        }

        if (!HasHeadroom(ledgeTop))
        {
            return false;
        }

        startPos = _rb.position;
        landingPos = MantleMath.ComputeLandingPosition(ledgeTop, facing, _landingForwardOffset);
        return true;
    }

    private bool HasHeadroom(Vector3 ledgeTop)
    {
        Vector3 checkPoint = ledgeTop + Vector3.up * _ledgeTopClearanceHeight;
        bool blocked = Physics.CheckSphere(checkPoint, HeadroomCheckRadius, _mantleLayerMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    private Vector3 GetFacingDirection()
    {
        float yaw = _mainCamera.transform.eulerAngles.y;
        Quaternion cameraYawRotation = Quaternion.Euler(0f, yaw, 0f);
        return (cameraYawRotation * Vector3.forward).normalized;
    }

    private void StartMantle(Vector3 startPos, Vector3 landingPos, Vector3 facing)
    {
        _isMantling = true;
        _timer = 0f;
        _startPos = startPos;
        _endPos = landingPos;
        _mantleFacing = facing;

        _originalGravitySetting = _rb.useGravity;
        _originalKinematicSetting = _rb.isKinematic;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        // Drive the vault purely by MovePosition; kinematic prevents collision resistance/stutter.
        _rb.isKinematic = true;
    }

    private void UpdateMantle()
    {
        _timer += Time.fixedDeltaTime;
        float normalizedTime = _timer / _mantleDuration;

        Vector3 nextPosition = MantleMath.SampleMantlePosition(
            _startPos,
            _endPos,
            _horizontalCurve,
            _verticalCurve,
            normalizedTime);

        _rb.MovePosition(nextPosition);

        if (normalizedTime >= 1f)
        {
            EndMantle();
        }
    }

    private void EndMantle()
    {
        _isMantling = false;
        _timer = 0f;

        // Restore physics before applying exit velocity (kinematic bodies ignore velocity).
        _rb.isKinematic = _originalKinematicSetting;
        _rb.useGravity = _originalGravitySetting;
        _rb.velocity = _mantleFacing * _exitForwardSpeed;

        _playerMovement.AddMomentum(_momentumGain);
        _cooldownTimer = _mantleCooldown;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_isMantling)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_startPos, _endPos);
        Gizmos.DrawSphere(_endPos, 0.15f);
    }
}
