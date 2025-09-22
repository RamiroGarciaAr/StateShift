using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IMovable, IJump
{
    [Header("References")]
    public Transform orientation;
    [Tooltip("Punto desde el que se chequea el suelo. Si está vacío, se calcula en runtime.")]
    public Transform feet;

    [Header("Drag")]
    public float groundDrag = 4f;
    public float airDrag    = 0.1f;

    [Header("Movement Settings")]
    public float walkSpeed   = 30f;  
    public float sprintSpeed = 100f;

    [Header("Acceleration")]
    [SerializeField] private float groundAcceleration = 60f; 
    [SerializeField] private float airAcceleration    = 24f; 
    [SerializeField] private float airControlFactor   = 0.4f; 

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.6f; 

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

        if (groundMask == 0)
            groundMask = LayerMask.GetMask("Ground");

    }

    void FixedUpdate()
    {
        DoGroundCheck();
        rb.drag = isGrounded ? groundDrag : airDrag;

        if (jumpRequested)
        {
            jumpRequested = false;
            if (isGrounded)
            {
                float v = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * v, ForceMode.VelocityChange);
            }
        }
        SpeedControl();

    }
    void Update()
{
    if (isDebugModeOn)
    {
        Debug.Log($"Sprinting: {isSprinting}, Speed: {rb.velocity.magnitude:F2}");
    }
}

    // ===== IMovable / IJump =====

    public void SetSprint(bool value) => isSprinting = value;

    public void MovePlayer(Vector2 input)
    {
        currentMaxSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Move(input, currentMaxSpeed);
    }

    public void Move(Vector3 inputAxis, float maxSpeed)
    {
        Vector3 wishDir;

        Vector3 fwd = Vector3.ProjectOnPlane(orientation.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(orientation.right, Vector3.up).normalized;
        wishDir = right * inputAxis.x + fwd * inputAxis.y;
    
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();



        rb.AddForce(wishDir * maxSpeed, ForceMode.VelocityChange);
    }


    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentMaxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y,limitedVel.z);

        }
    }

    public void Jump() => jumpRequested = true;

    // ===== Ground check =====
    private void DoGroundCheck()
    {
        Vector3 center = (feet != null) ? feet.position : (rb.position + Vector3.down * groundCheckDistance);

        // CheckSphere: más barato que SphereCast cuando solo te importa “¿estoy tocando suelo?”
        isGrounded = Physics.CheckSphere(center, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (isDebugModeOn)
        {
            Color c = isGrounded ? Color.green : Color.red;
            Debug.DrawLine(center, center + Vector3.up * 0.1f, c, 0f, false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!isDebugModeOn) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 center = (feet != null) ? feet.position : transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}
