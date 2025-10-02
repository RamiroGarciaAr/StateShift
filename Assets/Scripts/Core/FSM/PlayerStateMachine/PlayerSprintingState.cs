using UnityEngine;

public class PlayerSprintingState : PlayerStateBase
{
    public PlayerSprintingState(PlayerController context) : base(context) { }

    public override void OnEnter()
    {
        if (Context.IsDebugModeOn)
            Log("Entering Sprinting State");
    }

    public override void OnUpdate()
    {
        CheckTransitions();
    }

    public override void OnFixedUpdate()
    {
        // Aplicar drag en suelo
        rb.drag = Context.GroundDrag;
        
        // Aplicar movimiento con velocidad de sprint
        if (MoveInput.sqrMagnitude > 0.01f)
        {
            Vector3 direction = CalculateMovementDirection(MoveInput);
            ApplyMovement(direction, SprintSpeed, GroundAcceleration);
        }
    }

    public override void CheckTransitions()
    {
        // Si no hay input → Idle
        if (MoveInput.sqrMagnitude <= 0.01f && IsGrounded)
        {
            Context.ChangeState(PlayerStateType.Idle);
            return;
        }

        // Si deja de sprintar → Walking
        if (!IsSprinting && IsGrounded && MoveInput.sqrMagnitude > 0.01f)
        {
            Context.ChangeState(PlayerStateType.Walking);
            return;
        }

        // Si no está en el suelo → Falling o Jumping
        if (!IsGrounded)
        {
            if (rb.velocity.y > 0.1f)
                Context.ChangeState(PlayerStateType.Jumping);
            else
                Context.ChangeState(PlayerStateType.Falling);
            return;
        }
    }

    public override void OnExit()
    {
        if (Context.IsDebugModeOn)
            Log("Exiting Sprinting State");
    }
}