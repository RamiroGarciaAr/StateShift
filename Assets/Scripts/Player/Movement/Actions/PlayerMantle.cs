using StateShift.Player.MantleLogic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMantle : MonoBehaviour
{
    private const float MinMantleDuration = 0.01f;
    private const float MinDirectionSqrMagnitude = 0.0001f;
    private const float MinProbeRadius = 0.01f;
    private const float ProbeSurfaceOffset = 0.02f;
    private const float LandingProbeExtraHeight = 0.5f;
    private const float LandingProbeDropDistance = 2f;
    private const float CapsuleRadiusScale = 0.9f;
    private const int OverlapBufferSize = 16;

    [Header("Detection")]
    [Tooltip("How far forward the player probes for a mantleable wall face.")]
    [SerializeField] private float _forwardCheckDistance = 1.1f;

    [Tooltip("Preferred height above the feet for the forward wall probe. The probe is clamped below the minimum ledge height.")]
    [SerializeField] private float _forwardRayHeight = 0.9f;

    [Tooltip("Radius of the forward sphere cast. This allows beveled and convex props to be detected reliably.")]
    [SerializeField] private float _wallProbeRadius = 0.15f;

    [Tooltip("Lowest ledge height from the player's feet that can be mantled.")]
    [SerializeField] private float _minLedgeHeight = 0.75f;

    [Tooltip("Highest ledge height from the player's feet that can be mantled.")]
    [SerializeField] private float _maxLedgeHeight = 1.6f;

    [Tooltip("Vertical clearance required above the landing surface.")]
    [SerializeField] private float _ledgeTopClearanceHeight = 1f;

    [Tooltip("Distance through the wall face used to locate its top surface.")]
    [SerializeField] private float _lipProbeForward = 0.3f;

    [Tooltip("Distance through or across the obstacle used to find a capsule-clear landing. Narrow props are vaulted completely.")]
    [SerializeField] private float _landingProbeForwardDistance = 1.25f;

    [Range(0f, 1f)]
    [Tooltip("Maximum upward component accepted for the wall face normal.")]
    [SerializeField] private float _maxWallUpDot = 0.55f;

    [Range(0f, 1f)]
    [Tooltip("Minimum upward component required for a mantle top or landing surface.")]
    [SerializeField] private float _minTopUpDot = 0.55f;

    [Tooltip("Layers considered valid mantleable geometry.")]
    [SerializeField] private LayerMask _mantleLayerMask = ~0;

    [Header("Input")]
    [Tooltip("Time after pressing Jump during which a mantle may start while the player rises toward the ledge.")]
    [SerializeField] private float _mantleInputBufferDuration = 0.25f;

    [Header("Grapple Auto-Mantle")]
    [Tooltip("Forward detection reach used when auto-mantling on grapple end.")]
    [SerializeField] private float _grappleMantleReach = 4f;

    [Tooltip("Maximum ledge height accepted when auto-mantling on grapple end.")]
    [SerializeField] private float _grappleMaxLedgeHeight = 2.5f;

    [Header("Trajectory")]
    [Tooltip("Total duration of the scripted mantle motion, in seconds.")]
    [SerializeField] private float _mantleDuration = 0.45f;

    [Tooltip("Curve driving horizontal progress over the mantle.")]
    [SerializeField] private AnimationCurve _horizontalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Curve controlling lift toward the trajectory apex. The trajectory forces a smooth descent near completion.")]
    [SerializeField] private AnimationCurve _verticalCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.4f, 1f),
        new Keyframe(0.7f, 1f),
        new Keyframe(1f, 0f));

    [Tooltip("Additional feet clearance above the detected obstacle top at the trajectory apex.")]
    [SerializeField] private float _trajectoryTopClearance = 0.1f;

    [Tooltip("Forward exit speed applied on completion.")]
    [SerializeField] private float _exitForwardSpeed = 3f;

    [Header("Momentum")]
    [Range(0f, 1f)]
    [Tooltip("Momentum burst added when the mantle completes.")]
    [SerializeField] private float _momentumGain = 0.15f;

    [Header("Cooldown")]
    [Tooltip("Time before another mantle can be triggered.")]
    [SerializeField] private float _mantleCooldown = 0.4f;

    private readonly Collider[] _overlapBuffer = new Collider[OverlapBufferSize];

    private Rigidbody _rb;
    private PlayerMovement _playerMovement;
    private CapsuleCollider _capsuleCollider;
    private Camera _mainCamera;

    private bool _isMantling;
    private float _timer;
    private float _cooldownTimer;
    private float _inputBufferTimer;
    private bool _originalGravitySetting;
    private bool _originalKinematicSetting;
    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _mantleFacing;
    private float _apexY;
    private RaycastHit _wallHit;
    private RaycastHit _topHit;
    private RaycastHit _landingHit;

    /// <summary>True once the cooldown has elapsed and no mantle is running.</summary>
    public bool CanMantle => _cooldownTimer <= 0f && !_isMantling;

    /// <summary>True while the scripted mantle trajectory is running.</summary>
    public bool IsMantling => _isMantling;

    /// <summary>True while a buffered Jump press may still start a mantle.</summary>
    public bool HasBufferedRequest => _inputBufferTimer > 0f;

    /// <summary>Normalized progress through the current mantle.</summary>
    public float MantleProgress01 => _isMantling ? Mathf.Clamp01(_timer / _mantleDuration) : 0f;

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

        if (_inputBufferTimer > 0f)
        {
            _inputBufferTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (_isMantling)
        {
            UpdateMantle();
        }
    }

    /// <summary>Buffers a Jump-triggered mantle request so detection can succeed during the initial jump rise.</summary>
    public void BufferMantleRequest()
    {
        _inputBufferTimer = Mathf.Max(0f, _mantleInputBufferDuration);
    }

    /// <summary>Attempts a jump-triggered mantle while a buffered request is active.</summary>
    public bool TryStartBufferedMantle()
    {
        if (!HasBufferedRequest || !CanMantle)
        {
            return false;
        }

        if (!TryStartMantleInternal(_forwardCheckDistance, _maxLedgeHeight))
        {
            return false;
        }

        _inputBufferTimer = 0f;
        return true;
    }

    /// <summary>Returns whether the standard jump mantle probe currently finds a valid destination.</summary>
    public bool HasMantleableLedge()
    {
        return DetectLedge(_forwardCheckDistance, _maxLedgeHeight, out _, out _, out _, out _);
    }

    /// <summary>Attempts to begin a jump-triggered mantle immediately.</summary>
    public bool TryStartMantle()
    {
        return TryStartMantleInternal(_forwardCheckDistance, _maxLedgeHeight);
    }

    /// <summary>Returns whether the grapple mantle probe currently finds a valid destination.</summary>
    public bool HasMantleableLedgeFromGrapple()
    {
        return DetectLedge(_grappleMantleReach, _grappleMaxLedgeHeight, out _, out _, out _, out _);
    }

    /// <summary>Attempts to begin an automatic mantle at grapple end.</summary>
    public bool TryStartMantleFromGrapple()
    {
        return TryStartMantleInternal(_grappleMantleReach, _grappleMaxLedgeHeight);
    }

    /// <summary>Aborts an in-progress mantle and restores the saved Rigidbody settings.</summary>
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

    private bool TryStartMantleInternal(float forwardCheckDistance, float maxLedgeHeight)
    {
        if (!CanMantle)
        {
            return false;
        }

        if (!DetectLedge(
                forwardCheckDistance,
                maxLedgeHeight,
                out Vector3 startPos,
                out Vector3 landingPos,
                out Vector3 facing,
                out float apexY))
        {
            return false;
        }

        StartMantle(startPos, landingPos, facing, apexY);
        return true;
    }

    private bool DetectLedge(
        float forwardCheckDistance,
        float maxLedgeHeight,
        out Vector3 startPos,
        out Vector3 landingPos,
        out Vector3 facing,
        out float apexY)
    {
        startPos = _rb.position;
        landingPos = _rb.position;
        facing = transform.forward;
        apexY = _rb.position.y;

        if (_capsuleCollider == null)
        {
            return false;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                return false;
            }
        }

        Vector3 viewDirection = GetFacingDirection();
        float feetY = _capsuleCollider.bounds.min.y;
        float probeRadius = Mathf.Max(MinProbeRadius, _wallProbeRadius);
        float maxProbeHeight = Mathf.Max(probeRadius + ProbeSurfaceOffset, _minLedgeHeight - probeRadius * 0.5f);
        float probeHeight = Mathf.Clamp(_forwardRayHeight, probeRadius + ProbeSurfaceOffset, maxProbeHeight);
        Vector3 wallOrigin = new Vector3(_rb.position.x, feetY + probeHeight, _rb.position.z);

        if (!Physics.SphereCast(wallOrigin, probeRadius, viewDirection, out _wallHit, forwardCheckDistance, _mantleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (Mathf.Abs(Vector3.Dot(_wallHit.normal, Vector3.up)) > _maxWallUpDot)
        {
            return false;
        }

        Vector3 inwardDirection = Vector3.ProjectOnPlane(-_wallHit.normal, Vector3.up);
        if (inwardDirection.sqrMagnitude < MinDirectionSqrMagnitude)
        {
            return false;
        }

        inwardDirection.Normalize();
        float topProbeY = feetY + maxLedgeHeight + _ledgeTopClearanceHeight;
        Vector3 topOrigin = new Vector3(_wallHit.point.x, topProbeY, _wallHit.point.z) + inwardDirection * _lipProbeForward;
        float topProbeDistance = maxLedgeHeight + _ledgeTopClearanceHeight;

        if (!Physics.Raycast(topOrigin, Vector3.down, out _topHit, topProbeDistance, _mantleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (Vector3.Dot(_topHit.normal, Vector3.up) < _minTopUpDot)
        {
            return false;
        }

        float ledgeHeight = _topHit.point.y - feetY;
        if (!MantleMath.IsLedgeHeightValid(ledgeHeight, _minLedgeHeight, maxLedgeHeight))
        {
            return false;
        }

        Vector3 landingProbeOrigin = new Vector3(_wallHit.point.x, topProbeY + LandingProbeExtraHeight, _wallHit.point.z)
            + inwardDirection * Mathf.Max(_landingProbeForwardDistance, _lipProbeForward);
        float landingProbeDistance = maxLedgeHeight + _ledgeTopClearanceHeight + LandingProbeExtraHeight + LandingProbeDropDistance;

        if (!Physics.Raycast(landingProbeOrigin, Vector3.down, out _landingHit, landingProbeDistance, _mantleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (Vector3.Dot(_landingHit.normal, Vector3.up) < _minTopUpDot)
        {
            return false;
        }

        Vector3 destination = _landingHit.point + Vector3.up * ProbeSurfaceOffset;
        if (!HasCapsuleClearance(destination))
        {
            return false;
        }

        startPos = _rb.position;
        landingPos = destination;
        facing = inwardDirection;
        apexY = Mathf.Max(_topHit.point.y + _trajectoryTopClearance, startPos.y, landingPos.y);
        return true;
    }

    private bool HasCapsuleClearance(Vector3 feetPosition)
    {
        Bounds bounds = _capsuleCollider.bounds;
        float radius = Mathf.Max(MinProbeRadius, _capsuleCollider.radius * GetHorizontalScale() * CapsuleRadiusScale);
        float height = Mathf.Max(radius * 2f, bounds.size.y);
        Vector3 bottom = feetPosition + Vector3.up * (radius + ProbeSurfaceOffset);
        Vector3 top = feetPosition + Vector3.up * (height - radius);
        int overlapCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, _overlapBuffer, _mantleLayerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < overlapCount; i++)
        {
            Collider overlap = _overlapBuffer[i];
            if (overlap == null || overlap == _capsuleCollider || overlap.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private float GetHorizontalScale()
    {
        Vector3 scale = transform.lossyScale;
        return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
    }

    private Vector3 GetFacingDirection()
    {
        Vector3 forward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up);
        return forward.sqrMagnitude > MinDirectionSqrMagnitude ? forward.normalized : transform.forward;
    }

    private void StartMantle(Vector3 startPos, Vector3 landingPos, Vector3 facing, float apexY)
    {
        _isMantling = true;
        _inputBufferTimer = 0f;
        _timer = 0f;
        _startPos = startPos;
        _endPos = landingPos;
        _mantleFacing = facing;
        _apexY = apexY;

        _originalGravitySetting = _rb.useGravity;
        _originalKinematicSetting = _rb.isKinematic;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;
    }

    private void UpdateMantle()
    {
        _timer += Time.fixedDeltaTime;
        float normalizedTime = _timer / _mantleDuration;
        Vector3 nextPosition = MantleMath.SampleMantlePositionWithApex(
            _startPos,
            _endPos,
            _apexY,
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
        _rb.isKinematic = _originalKinematicSetting;
        _rb.useGravity = _originalGravitySetting;
        _rb.velocity = _mantleFacing * _exitForwardSpeed;
        _playerMovement.AddMomentum(_momentumGain);
        _cooldownTimer = _mantleCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = _isMantling ? Color.green : Color.yellow;
        Gizmos.DrawLine(_startPos, _endPos);
        Gizmos.DrawSphere(_endPos, 0.15f);
    }
}
