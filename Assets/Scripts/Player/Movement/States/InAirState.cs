using Core;

public class InAirState : BaseState<PlayerMovementContext>
{
    public InAirState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.InAir);
    }

    public override void OnUpdate()
    {
        // Land
        if (Context.PlayerMovement.IsGrounded)
        {
            Context.StateMachine.ChangeState(MovementState.Grounded);
            return;
        }

        // Wall run
        if (Context.PlayerWallRun.CanWallRun())
        {
            Context.StateMachine.ChangeState(MovementState.WallRunning);
            return;
        }

        // Grapple
        if (Context.WantsToGrapple && Context.PlayerGrapple.CanGrapple)
        {
            if (Context.PlayerGrapple.TryStartGrapple())
            {
                Context.StateMachine.ChangeState(MovementState.Grappling);
                return;
            }
        }

        // Dash
        if (Context.WantsToDash)
        {
            Context.WantsToDash = false;
            if (Context.PlayerDash.TryStartDash(Context.DashInputDirection))
            {
                Context.StateMachine.ChangeState(MovementState.Dashing);
                return;
            }
        }
    }
}
