using UnityEngine;
public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerController_Legacy context) : base(context) { }

    public override void OnEnter()
    {
        if (Context.IsDebugModeOn)
            Log("Entering Idle State");
    }

    public override void OnUpdate()
    {
        CheckTransitions();
    }

    public override void OnFixedUpdate()
    {
        // Aplicar drag en idle
        rb.drag = Context.GroundDrag;
    }

    public override void CheckTransitions()
    {
        // Si hay input de movimiento → Walking
        if (MoveInput.sqrMagnitude > 0.01f && IsGrounded)
        {
            Context.ChangeState(PlayerStateType.Walking);
            return;
        }

        // Si no está en el suelo → Falling
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
            Log("Exiting Idle State");
    }
}