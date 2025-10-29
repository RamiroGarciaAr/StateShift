using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroundChecker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float legHeight = 0.5f;
    [SerializeField] private LayerMask groundLayerMask;
    
    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;

    private Rigidbody _rb;
    private float _lastAirVelocityY;

    #region Properties
    public bool IsGrounded { get; private set; }
    public bool WasGrounded { get; private set; }
    public Rigidbody GroundRigidbody { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    public Vector3 GroundPoint { get; private set; }
    public float LastAirVelocityY => _lastAirVelocityY;
    
    public Vector3 GroundVelocity
    {
        get
        {
            if (GroundRigidbody != null)
                return GroundRigidbody.GetPointVelocity(GroundPoint);
            return Vector3.zero;
        }
    }

    public float LegHeight
    {
        get => legHeight;
        set => legHeight = value;
    }

    public CapsuleCollider Collider
    {
        get => capsuleCollider;
        set => capsuleCollider = value;
    }
    #endregion

    #region Events
    public event System.Action OnLanded;
    public event System.Action OnLeftGround;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }
    }

    private void Start()
    {
        ModifyColliderHeight();
    }

    public void CheckGround()
    {
        var origin = capsuleCollider.bounds.center;
        var maxDistance = Vector3.Distance(origin, _rb.position + groundCheckDistance * Vector3.down);
        var ray = new Ray(origin, Vector3.down);

        WasGrounded = IsGrounded;
        if (!IsGrounded) 
        {
            _lastAirVelocityY = _rb.velocity.y;
        }

        if (Physics.Raycast(ray, out var hit, maxDistance, groundLayerMask) && !hit.collider.isTrigger)
        {
            Debug.DrawLine(origin, origin + Vector3.down * maxDistance, Color.green);

            IsGrounded = true;
            GroundPoint = hit.point;
            GroundNormal = hit.normal;
            GroundRigidbody = hit.rigidbody;

            // Detectar aterrizaje
            if (!WasGrounded && IsGrounded)
            {
                OnLanded?.Invoke();
            }
        }
        else
        {
            Debug.DrawLine(origin, origin + Vector3.down * groundCheckDistance, Color.red);

            bool wasGroundedBefore = IsGrounded;
            IsGrounded = false;
            GroundPoint = transform.position;
            GroundNormal = Vector3.up;
            GroundRigidbody = null;

            // Detectar cuando dejamos el suelo
            if (wasGroundedBefore && !IsGrounded)
            {
                OnLeftGround?.Invoke();
            }
        }
    }

    private void ModifyColliderHeight()
    {
        if (LegHeight >= capsuleCollider.height)
        {
            Debug.LogError("Leg height is greater than collider height", this);
            return;
        }

        var newHeight = capsuleCollider.height - legHeight;
        var newCenter = capsuleCollider.center + Vector3.up * (capsuleCollider.height - newHeight) * .5f;
        capsuleCollider.height = newHeight;
        capsuleCollider.center = newCenter;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying || capsuleCollider == null) return;

        var origin = capsuleCollider.bounds.center;
        var maxDistance = Vector3.Distance(origin, transform.position + groundCheckDistance * Vector3.down);
        var extent = origin + Vector3.down * maxDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, extent);
    }
}