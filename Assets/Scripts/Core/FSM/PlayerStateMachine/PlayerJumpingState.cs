using UnityEngine;


public class PlayerJumpingState : PlayerStateBase
{
    private float jumpStartTime;
    private bool canCutJump;
    private float originalGravityY;

    public PlayerJumpingState(PlayerController_Legacy context) : base(context) { }

    public override void OnEnter()
    {
        if (Context.IsDebugModeOn)
            Log("Entering Jumping State");

        jumpStartTime = Time.time;
        canCutJump = false;
        originalGravityY = Physics.gravity.y;

        // Aplicar impulso inicial del salto
        float jumpVelocity = CalculateJumpVelocity(Context.MinJumpHeight);
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
    }

    public override void OnUpdate()
    {
        CheckTransitions();
    }

    public override void OnFixedUpdate()
    {
        // Aplicar drag en aire
        rb.drag = Context.AirDrag;

        // Control de movimiento en el aire
        if (MoveInput.sqrMagnitude > 0.01f)
        {
            Vector3 direction = CalculateMovementDirection(MoveInput);
            float maxSpeed = IsSprinting ? SprintSpeed : WalkSpeed;
            
            // Reducir control en el aire
            Vector3 targetVelocity = direction * maxSpeed;
            Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            Vector3 velocityDifference = (targetVelocity - currentVelocity) * AirControlFactor;
            
            float maxVelocityChange = AirAcceleration * Time.fixedDeltaTime;
            Vector3 velocityChange = Vector3.ClampMagnitude(velocityDifference, maxVelocityChange);
            
            Vector3 newVelocity = currentVelocity + velocityChange;
            rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
        }

        HandleJumpPhysics();
    }

    private void HandleJumpPhysics()
    {
        float jumpTime = Time.time - jumpStartTime;

        // Habilitar corte de salto después del tiempo mínimo
        if (!canCutJump && jumpTime >= Context.MinJumpTime)
        {
            canCutJump = true;
        }

        // Variable jump height - mantener mientras se sostiene el botón
        if (Context.IsJumpHeld && rb.velocity.y > 0f && jumpTime < Context.JumpRiseTime)
        {
            float progress = jumpTime / Context.JumpRiseTime;
            float targetHeight = Mathf.Lerp(Context.MinJumpHeight, Context.MaxJumpHeight, progress);
            float targetVelocity = CalculateJumpVelocity(targetHeight);

            float currentVelocity = rb.velocity.y;
            float adjustedVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * 8f);

            rb.velocity = new Vector3(rb.velocity.x, adjustedVelocity, rb.velocity.z);
        }

        // Jump cut si se suelta el botón
        if (!Context.IsJumpHeld && canCutJump && rb.velocity.y > 0.1f)
        {
            ApplyJumpCut();
        }
    }

    private void ApplyJumpCut()
    {
        rb.velocity = new Vector3(
            rb.velocity.x,
            rb.velocity.y * (1f / Context.JumpCutMultiplier),
            rb.velocity.z
        );

        float extraGravity = originalGravityY * (Context.LowJumpMultiplier - 1f);
        rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);

        canCutJump = false;
    }

    public override void CheckTransitions()
    {
        // Si la velocidad Y es negativa → Falling
        if (rb.velocity.y <= 0f)
        {
            Context.ChangeState(PlayerStateType.Falling);
            return;
        }

        // Si detecta una pared → WallRunning
        WallRunHit wallHit = Context.DetectWall();
        if (wallHit.hit && MoveInput.sqrMagnitude > 0.1f)
        {
            Context.ChangeState(PlayerStateType.WallRunning);
            return;
        }

        // Si toca el suelo mientras salta (techo bajo)
        if (IsGrounded)
        {
            if (MoveInput.sqrMagnitude > 0.01f)
            {
                if (IsSprinting)
                    Context.ChangeState(PlayerStateType.Sprinting);
                else
                    Context.ChangeState(PlayerStateType.Walking);
            }
            else
            {
                Context.ChangeState(PlayerStateType.Idle);
            }
        }
    }

    private float CalculateJumpVelocity(float height)
    {
        return Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * height);
    }

    public override void OnExit()
    {
        if (Context.IsDebugModeOn)
            Log("Exiting Jumping State");
    }
}