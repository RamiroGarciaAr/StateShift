using UnityEngine;


public class PlayerFallingState : PlayerStateBase
{
    private float originalGravityY;

    public PlayerFallingState(PlayerController_Legacy context) : base(context) { }

    public override void OnEnter()
    {
        if (Context.IsDebugModeOn)
            Log("Entering Falling State");

        originalGravityY = Physics.gravity.y;
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

        // Aplicar gravedad extra para caída más rápida (mejor game feel)
        ApplyFallGravity();
    }

    private void ApplyFallGravity()
    {
        float extraGravity = originalGravityY * (Context.FallGravityMultiplier - 1f);
        rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
    }

    public override void CheckTransitions()
    {
        // Si toca el suelo → Idle o Walking
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
            return;
        }

        // Si detecta una pared y hay input de movimiento → WallRunning
        WallRunHit wallHit = Context.DetectWall();
        if (wallHit.hit && MoveInput.sqrMagnitude > 0.1f)
        {
            Context.ChangeState(PlayerStateType.WallRunning);
            return;
        }

        // Si por alguna razón la velocidad Y es positiva → Jumping
        if (rb.velocity.y > 0.1f)
        {
            Context.ChangeState(PlayerStateType.Jumping);
        }
    }

    public override void OnExit()
    {
        if (Context.IsDebugModeOn)
            Log("Exiting Falling State");
    }
}