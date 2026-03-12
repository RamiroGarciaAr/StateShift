using Core;
using UnityEngine;

public class DashingState : BaseState<PlayerMovementContext>
{
    public DashingState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Dashing);

        // Dash is already started by the transition state
        // This state just manages behavior during the dash
    }

    public override void OnUpdate()
    {
        // Exit dash when it's complete
        if (!Context.PlayerDash.IsDashing)
        {
            ExitToAppropriateState();
            return;
        }
        // Transition to Grapple
        if (Context.WantsToGrapple && Context.PlayerGrapple.CanGrapple)
        {
            Context.PlayerDash.CancelDash();
            bool grappleStarted = Context.PlayerGrapple.TryStartGrapple();
            if (grappleStarted)
            {
                Context.StateMachine.ChangeState(MovementState.Grappling);
                return;
            }
        }
    }

    public override void OnExit()
    {
        // Ensure dash is properly ended
        if (Context.PlayerDash.IsDashing)
        {
            Context.PlayerDash.CancelDash();
        }
    }

    private void ExitToAppropriateState()
    {
        Context.StateMachine.ChangeState(MovementState.Grounded);
    }
}