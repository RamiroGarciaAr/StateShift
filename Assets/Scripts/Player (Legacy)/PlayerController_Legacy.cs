using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlatformRider))]
public class PlayerController_Legacy : MonoBehaviour, IMovable, IJump
{
    [Header("References")]
    public Transform orientation;
    [Tooltip("Punto desde el que se chequea el suelo. Si está vacío, se calcula en runtime.")]
    public Transform feet;

    [Header("Drag")]
    public float groundDrag = 4f;
    public float airDrag = 0.1f;

    [Header("Movement Settings")]
    public float walkSpeed = 30f;
    public float sprintSpeed = 100f;

    [Header("Movement Physics")]
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float airAcceleration = 24f;
    [SerializeField] private float airControlFactor = 0.4f;

    [Header("Jump Settings")]
    [SerializeField] private float minJumpHeight = 1.2f;
    [SerializeField] private float maxJumpHeight = 2.8f;
    [SerializeField] private float jumpRiseTime = 0.3f;
    [SerializeField] private float minJumpTime = 0.08f;

    [Header("Jump Feel")]
    [SerializeField] private float jumpCutMultiplier = 2.5f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float lowJumpMultiplier = 3f;

    [Header("Coyote & Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Wall Running")]
    [SerializeField] private float wallRunSpeed = 35f;
    [SerializeField] private float wallRunAcceleration = 50f;
    [SerializeField] private float wallRunDrag = 0.5f;
    [SerializeField] private float wallStickForce = 20f;
    [SerializeField] private float wallRunGravityMultiplier = 0.3f;
    [SerializeField] private float maxWallRunTime = 2f;
    [SerializeField] private float minWallRunSpeed = 5f;
    [SerializeField] private float maxWallRunFallSpeed = 3f;
    [SerializeField] private float wallRunExitBoost = 5f;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private float minWallRunHeight = 1f;
    [SerializeField] private LayerMask wallMask;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundMask;

    [Header("Debug")]
    public bool isDebugModeOn = false;

    // State Machine
    private StateMachine<PlayerStateType> stateMachine;

    // Componentes
    private Rigidbody rb;
    private PlatformRider rider;

    // Estado interno
    private bool isGrounded;
    private bool isSprinting;
    private bool isJumpHeld;
    private Vector2 moveInput;
    private bool jumpRequested;
    
    // Timers
    private float lastGroundedTime;
    private float jumpBufferTimer;

    // Properties públicas para los estados
    public Rigidbody Rb => rb;
    public Transform Orientation => orientation;
    public float GroundDrag => groundDrag;
    public float AirDrag => airDrag;
    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;
    public float GroundAcceleration => groundAcceleration;
    public float AirAcceleration => airAcceleration;
    public float AirControlFactor => airControlFactor;
    public float MinJumpHeight => minJumpHeight;
    public float MaxJumpHeight => maxJumpHeight;
    public float JumpRiseTime => jumpRiseTime;
    public float MinJumpTime => minJumpTime;
    public float JumpCutMultiplier => jumpCutMultiplier;
    public float FallGravityMultiplier => fallGravityMultiplier;
    public float LowJumpMultiplier => lowJumpMultiplier;
    public float WallRunSpeed => wallRunSpeed;
    public float WallRunAcceleration => wallRunAcceleration;
    public float WallRunDrag => wallRunDrag;
    public float WallStickForce => wallStickForce;
    public float WallRunGravityMultiplier => wallRunGravityMultiplier;
    public float MaxWallRunTime => maxWallRunTime;
    public float MinWallRunSpeed => minWallRunSpeed;
    public float MaxWallRunFallSpeed => maxWallRunFallSpeed;
    public float WallRunExitBoost => wallRunExitBoost;
    public bool IsGrounded => isGrounded;
    public bool IsSprinting => isSprinting;
    public bool IsJumpHeld => isJumpHeld;
    public Vector2 MoveInput => moveInput;
    public bool IsDebugModeOn => isDebugModeOn;

    void OnValidate()
    {
        if (groundMask == 0)
            groundMask = LayerMask.GetMask("Ground");

        if (wallMask == 0)
            wallMask = LayerMask.GetMask("Wall");

        if (groundCheckRadius < 0.05f) groundCheckRadius = 0.05f;
        if (groundCheckDistance < groundCheckRadius) groundCheckDistance = groundCheckRadius + 0.05f;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        rider = GetComponent<PlatformRider>();

        if (groundMask == 0)
            groundMask = LayerMask.GetMask("Ground");

        if (wallMask == 0)
            wallMask = LayerMask.GetMask("Wall");

    }


    void Update()
    {
        DoGroundCheck();
        UpdateTimers();
        stateMachine.Update();

        // Debug
        if (isDebugModeOn)
        {
            Debug.Log($"Current State: {stateMachine.CurrentStateType}");
        }
    }

    void FixedUpdate()
    {
        HandleJumpRequest();
        stateMachine.FixedUpdate();
    }

    // ===== Ground Check =====
    private void DoGroundCheck()
    {
        Vector3 center = (feet != null) ? feet.position : (rb.position + Vector3.down * groundCheckDistance);
        isGrounded = Physics.CheckSphere(center, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded)
            lastGroundedTime = Time.time;

        if (isDebugModeOn)
        {
            Color c = isGrounded ? Color.green : Color.red;
            Debug.DrawLine(center, center + Vector3.up * 0.1f, c, 0f, false);
        }
    }

    // ===== Timers =====
    private void UpdateTimers()
    {
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;
    }

    // ===== Jump Handling =====
    private void HandleJumpRequest()
    {
        if (!jumpRequested) return;

        jumpRequested = false;

        // Coyote time check
        bool canJumpFromCoyote = Time.time - lastGroundedTime <= coyoteTime;

        if (isGrounded || canJumpFromCoyote)
        {
            // Cambiar al estado de salto
            ChangeState(PlayerStateType.Jumping);
        }
        else
        {
            // Buffer el salto
            jumpBufferTimer = jumpBufferTime;
        }
    }

    // Check del buffer en los estados que tocan suelo
    public bool HasJumpBuffered()
    {
        return jumpBufferTimer > 0f;
    }

    public void ConsumeJumpBuffer()
    {
        jumpBufferTimer = 0f;
    }

    // ===== IMovable / IJump =====
    public void SetSprint(bool value) => isSprinting = value;

    public void MovePlayer(Vector2 input)
    {
        moveInput = input;
    }

    public void Move(Vector3 inputAxis, float maxSpeed)
    {
        moveInput = new Vector2(inputAxis.x, inputAxis.z);
    }

    public void Jump()
    {
        jumpRequested = true;
    }

    public void SetJumpHeld(bool held)
    {
        isJumpHeld = held;
    }

    // ===== State Machine Control =====
    public void ChangeState(PlayerStateType newState)
    {
        stateMachine.ChangeState(newState);
    }

    public PlayerStateType GetCurrentState()
    {
        return stateMachine.CurrentStateType;
    }

    // ===== Wall Detection =====
    public WallRunHit DetectWall()
    {
        // Verificar que no esté en el suelo (debe estar en el aire)
        if (isGrounded)
            return WallRunHit.NoHit();

        // Verificar altura mínima
        if (transform.position.y < minWallRunHeight)
            return WallRunHit.NoHit();

        // Verificar velocidad mínima horizontal
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude < minWallRunSpeed)
            return WallRunHit.NoHit();

        // Raycast a la derecha
        if (Physics.Raycast(transform.position, orientation.right, out RaycastHit rightHit, wallCheckDistance, wallMask))
        {
            // Verificar que la pared sea aproximadamente vertical
            if (Vector3.Dot(rightHit.normal, Vector3.up) < 0.1f && Vector3.Dot(rightHit.normal, Vector3.up) > -0.1f)
            {
                return new WallRunHit(true, rightHit.normal, rightHit.point, true, rightHit.distance);
            }
        }

        // Raycast a la izquierda
        if (Physics.Raycast(transform.position, -orientation.right, out RaycastHit leftHit, wallCheckDistance, wallMask))
        {
            // Verificar que la pared sea aproximadamente vertical
            if (Vector3.Dot(leftHit.normal, Vector3.up) < 0.1f && Vector3.Dot(leftHit.normal, Vector3.up) > -0.1f)
            {
                return new WallRunHit(true, leftHit.normal, leftHit.point, false, leftHit.distance);
            }
        }

        return WallRunHit.NoHit();
    }

    // ===== Gizmos =====
    void OnDrawGizmosSelected()
    {
        if (!isDebugModeOn) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 center = (feet != null) ? feet.position : transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}