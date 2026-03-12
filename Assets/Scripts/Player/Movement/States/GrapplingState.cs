using Core;
using UnityEngine;

public class GrapplingState : BaseState<PlayerMovementContext>
{
    public GrapplingState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Grappling);

    }

    public override void OnUpdate()
    {
        // Exit grapple when it's complete
        if (!Context.PlayerGrapple.IsGrappling)
        {
            ExitToAppropriateState();
            return;
        }

        // Allow canceling grapple with jump
        if (Context.WantsToJump)
        {
            Context.PlayerGrapple.CancelGrapple();
            ExitToAppropriateState();
            return;
        }

        // Allow canceling grapple by releasing grapple button
        if (!Context.WantsToGrapple)
        {
            Context.PlayerGrapple.CancelGrapple();
            ExitToAppropriateState();
            return;
        }
    }

    public override void OnExit()
    {
        // Ensure grapple is properly ended
        if (Context.PlayerGrapple.IsGrappling)
        {
            Context.PlayerGrapple.CancelGrapple();
        }
    }

    private void ExitToAppropriateState()
    {
        Context.StateMachine.ChangeState(MovementState.Grounded);
    }
}