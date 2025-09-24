using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IMovable, IJump
{
    [Header("References")]
    public Transform orientation;
    [Tooltip("Punto desde el que se chequea el suelo. Si está vacío, se calcula en runtime.")]
    public Transform feet;

    [SerializeField] private WallDetection wallDetection = new();

    public const string PLATFORM_TAG = "MovingPlatform";

    [Header("Drag")]
    public float groundDrag = 4f;
    public float airDrag    = 0.1f;

    [Header("Movement Settings")]
    public float walkSpeed   = 30f;  
    public float sprintSpeed = 100f;

    [Header("Movement Strategy")]
    [SerializeField] private PlayerMovementStrategy movementStrategy = new();

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.6f;
    [SerializeField] private PlayerJumpStrategy jumpStrategy;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius   = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.5f; 
    [SerializeField] private LayerMask groundMask;                     


    [Header("Debug")]
    public bool isDebugModeOn = false;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isSprinting;
    private bool jumpRequested;
    private float currentMaxSpeed;

    void OnValidate()
    {
        if (groundMask == 0)
            groundMask = LayerMask.GetMask("Ground");


        if (groundCheckRadius < 0.05f) groundCheckRadius = 0.05f;
        if (groundCheckDistance < groundCheckRadius) groundCheckDistance = groundCheckRadius + 0.05f;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (groundMask == 0)  groundMask  = LayerMask.GetMask("Ground");

        jumpStrategy ??= new PlayerJumpStrategy(); 
    }
    void Update()
    {
        DoGroundCheck();
        DoPlatformCheck();
    }
    void FixedUpdate() {

        rb.drag = isGrounded ? groundDrag : airDrag;

        jumpStrategy.UpdateJump(rb);

        if (jumpRequested) {
            jumpRequested = false;
            if (jumpStrategy.CanJump(isGrounded)) {       
                jumpStrategy.ApplyJump(rb, jumpHeight);   
            }
        }
    }


    // ===== IMovable / IJump =====
    public void SetSprint(bool value) => isSprinting = value;

    public void MovePlayer(Vector2 input)
    {
        currentMaxSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 inputDirection = CalculateMovementDirection(input);
        movementStrategy.ApplyMovement(rb, inputDirection, currentMaxSpeed, isGrounded);
    }

    public void Move(Vector3 inputAxis, float maxSpeed)
    {
        Vector3 inputDirection = CalculateMovementDirection(new Vector2(inputAxis.x, inputAxis.z));
        movementStrategy.ApplyMovement(rb, inputDirection, maxSpeed, isGrounded);
    }

    private Vector3 CalculateMovementDirection(Vector2 input)
    {
        Vector3 forward = Vector3.ProjectOnPlane(orientation.forward, Vector3.up).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(orientation.right,   Vector3.up).normalized;
        Vector3 direction = right * input.x + forward * input.y;
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        return direction;
    }

    public void Jump() => jumpRequested = true;

    public void SetJumpHeld(bool held) {
        jumpStrategy?.SetJumpHeld(held);
    }
    // ===== Ground check =====
    private void DoGroundCheck()
    {
        Vector3 center = (feet != null) ? feet.position : (rb.position + Vector3.down * groundCheckDistance);
        isGrounded = Physics.CheckSphere(center, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (isDebugModeOn)
        {
            Color c = isGrounded ? Color.green : Color.red;
            Debug.DrawLine(center, center + Vector3.up * 0.1f, c, 0f, false);
        }
    }
    private bool DoPlatformCheck()
    {
        Vector3 center = (feet != null) ? feet.position : (rb.position + Vector3.down * groundCheckDistance);
        if (Physics.SphereCast(center, groundCheckRadius, Vector3.down, out RaycastHit hit, 0.2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag(PLATFORM_TAG))
            {
                Debug.Log("On Platform");
                return true;
            }
        }
        return false;
    }
    void OnDrawGizmosSelected()
    {
        if (!isDebugModeOn) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 center = (feet != null) ? feet.position : transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}
